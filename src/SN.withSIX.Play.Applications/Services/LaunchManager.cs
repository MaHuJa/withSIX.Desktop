// <copyright company="SIX Networks GmbH" file="LaunchManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Caliburn.Micro;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;
using ServerAddress = SN.withSIX.Play.Core.ServerAddress;

namespace SN.withSIX.Play.Applications.Services
{
    public class LaunchManager : IHandle<GameLaunchedEvent>, IHandle<ActiveGameChanged>, IEnableLogging,
        IDisposable, IDomainService
    {
        static readonly int CheckRunningInterval = 3.Seconds();
        static readonly Version defaultVersion = new Version(0, 0, 0);
        static readonly Regex connectRegex = new Regex(@"-connect=([\w\.]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex portRegex = new Regex(@"-port=(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        readonly IContentManager _contentManager;
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly IGameContext _gameContext;
        readonly IGameLauncherFactory _gameLaunchFactory;
        readonly Timer _listTimer;
        readonly IProcessManager _processManager;
        readonly UserSettings _settings;
        readonly object _updateLock = new Object();
        volatile bool _isUpdating;
        public DateTime? LastGameLaunch;

        public LaunchManager(IContentManager cm, IGameContext gameContext,
            UserSettings settings, IProcessManager processManager, IGameLauncherFactory gameLaunchFactory,
            IEventAggregator eventBus, IDialogManager dialogManager) {
            _contentManager = cm;
            _gameContext = gameContext;
            _settings = settings;
            _processManager = processManager;
            _gameLaunchFactory = gameLaunchFactory;
            _eventBus = eventBus;
            _dialogManager = dialogManager;
            TryHandleServerAddress();
            _listTimer = new TimerWithElapsedCancellation(CheckRunningInterval, CheckRunningTimerElapsed);
        }

        public void LaunchExternalApps() {
            var exceptions = new List<Exception>();
            _settings.AppOptions.ExternalApps.ToList()
                .ForEach(x => LaunchAppAndGatherException(x, exceptions));
            if (exceptions.Any()) {
                _eventBus.PublishOnCurrentThread(new InformationalUserError(new AggregateException(exceptions),
                    "Failure during launch of external apps, might not exist?", "Failure during launching external apps"));
            }
        }

        void LaunchAppAndGatherException(ExternalApp x, ICollection<Exception> exceptions) {
            try {
                LaunchApp(x);
            } catch (Exception e) {
                exceptions.Add(e);
            }
        }

        public async Task StartGame() {
            Exception e;
            try {
                await GetCurrentGame().Launch(_gameLaunchFactory).ConfigureAwait(false);
                return;
            } catch (OperationCanceledException ex) {
                this.Logger().Warn("User cancelled the launch process");
                return;
            } catch (Win32Exception ex) {
                if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                    throw;
                this.Logger().Warn("User cancelled the elevated launch process");
                return;
            } catch (UserException ex) {
                e = ex;
            }
            const string title = "A user error occurred during launch";
            this.Logger().FormattedWarnException(e, title);
            await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(e.Message, title));
        }

        public async Task JoinServer() {
            var server = GetCurrentGame().CalculatedSettings.Server;
            if (server == null) {
                UsageCounter.ReportUsage("Dialog - No active server to join");
                await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("No active server to join"));
                return;
            }

            await StartGame().ConfigureAwait(false);
        }

        public async Task JoinServer(ServerAddress address) {
            var server = _contentManager.ServerList.FindOrCreateServer(address);
            if (await ConfirmServerProtectionLevel(server))
                return;
            await SwitchAndJoinServer(server.ServerAddress).ConfigureAwait(false);
        }

        [Obsolete("Arma specific")]
        void TryHandleServerAddress() {
            try {
                var data = GetServerAddressFromRunningProcesses();
                if (data.Item2 == null)
                    return;
                var server = _contentManager.ServerList.FindOrCreateServer(data.Item2);
                server.TryUpdateAsync().ConfigureAwait(false);

                if (data.Item1 != null)
                    RegisterRunning(GetCurrentGame(), data.Item1, server);
                else {
                    var procs = _gameContext.Games.RunningProcesses().ToArray();
                    if (procs.Any()) {
                        using (new CompositeDisposable(procs.Except(new[] {procs[0]})))
                            RegisterRunning(GetCurrentGame(), procs[0]);
                    } else
                        new CompositeDisposable(procs).Dispose();
                }
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
        }

        Tuple<Process, ServerAddress> GetServerAddressFromRunningProcesses() {
            var installedState = GetCurrentGame().InstalledState;
            if (!installedState.IsInstalled)
                return new Tuple<Process, ServerAddress>(null, null);

            var processes = Tools.Processes.GetCommandlineArgs(installedState.Executable.ToString());
            try {
                var procs = processes
                    .Where((kvp, i) => !string.IsNullOrWhiteSpace(kvp.Value))
                    .ToDictionary(proc => proc.Key, proc => proc.Value);

                if (!procs.Any()) {
                    using (new CompositeDisposable(processes.Keys))
                        return new Tuple<Process, ServerAddress>(null, null);
                }

                var theOne = GetServerAddress(procs);
                using (new CompositeDisposable(processes.Keys.Except(new[] {theOne.Item1})))
                    return theOne;
            } catch {
                new CompositeDisposable(processes.Keys).Dispose();
                throw;
            }
        }

        static Game GetCurrentGame() {
            return DomainEvilGlobal.SelectedGame.ActiveGame;
        }

        static Tuple<Process, ServerAddress> GetServerAddress(Dictionary<Process, string> procs) {
            foreach (var pair in procs) {
                var match = connectRegex.Matches(pair.Value);
                if (match.Count == 0)
                    continue;
                var address = match[0].Groups[1].Value;
                match = portRegex.Matches(pair.Value);
                var port = match.Count > 0 ? match[0].Groups[1].Value.TryInt() : 2302;
                return Tuple.Create(pair.Key, new ServerAddress(IPAddress.Parse(address), port));
            }

            return new Tuple<Process, ServerAddress>(null, null);
        }

        void GameVersionCheck(Version gameVersion, string game) {
            /*
            var fam = _syncManager.Families.FirstOrDefault(x => x.Name == game);
            GameVersionCheckByVersion(gameVersion, fam == null ? null : fam.Standard);
*/
        }

        /*        void GameVersionCheckByVersion(Version gameVersion, string version) {
            if (gameVersion == null)
                return;
            var localVersion = gameVersion;
            var remoteVersion = Version.Parse(version);
            if (remoteVersion <= localVersion)
                return;
            if (_dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                String.Format(
                    "New game version available ({0}), please upgrade. Your current version is: {1}.\nYou can find patches at the official game support site: http://www.arma2.com/index.php?Itemid=20",
                    remoteVersion, localVersion), null, SixMessageBoxButton.YesNo)).IsYes()) {
                Tools.Generic.TryOpenUrl(
                    "http://www.arma2.com/index.php?Itemid=20");
            }
        }*/

        bool TryGameVersionCheck(Game game) {
            try {
                var installedState = game.InstalledState;
                if (!installedState.IsInstalled)
                    return false;
                GameVersionCheck(installedState.Version, installedState.Executable.FileNameWithoutExtension);
                return true;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
            return false;
        }

        async Task SwitchAndJoinServer(ServerAddress address) {
            var server = _contentManager.ServerList.FindOrCreateServer(address);
            GetCurrentGame().CalculatedSettings.Server = server;

            await Task.Delay(2000).ConfigureAwait(false);
            await JoinServer().ConfigureAwait(false);
        }

        async Task<bool> ConfirmServerProtectionLevel(Server server) {
            Contract.Requires<ArgumentNullException>(server != null);
            if ((server.Protection != ProtectionLevel.Low) && (server.Protection != ProtectionLevel.None))
                return false;
            UsageCounter.ReportUsage(String.Format("Dialog - Connecting to unprotected server: {0}",
                _settings.AppOptions.RememberWarnOnUnprotectedServer));

            if (!_settings.AppOptions.RememberWarnOnUnprotectedServer) {
                var r =
                    await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                        "The selected server is unprotected. Are you sure you want to connect?",
                        "Connecting to unprotected server...", SixMessageBoxButton.YesNo) {
                            RememberedState = false
                        });

                switch (r) {
                case SixMessageBoxResult.YesRemember:
                    _settings.AppOptions.RememberWarnOnUnprotectedServer = true;
                    _settings.AppOptions.WarnOnUnprotectedServer = true;
                    break;
                case SixMessageBoxResult.NoRemember:
                    _settings.AppOptions.RememberWarnOnUnprotectedServer = true;
                    _settings.AppOptions.WarnOnUnprotectedServer = false;
                    break;
                }

                if (!r.IsYes())
                    return true;
            } else {
                if (!_settings.AppOptions.WarnOnUnprotectedServer)
                    return true;
            }
            return false;
        }

        void LaunchApp(ExternalApp app) {
            app.LaunchWithChecks(_processManager,
                GetCurrentGame().CalculatedSettings.Server != null);
        }

        void RegisterRunning(Game game, Process proc, Server server = null, Collection collection = null) {
            game.RegisterRunning(proc);
            _eventBus.PublishOnCurrentThread(new MyActiveServerAddressChanged(server == null ? null : server.Address));
        }

        bool CheckRunningTimerElapsed() {
            try {
                CheckRunning();
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
            return true;
        }

        void CheckRunning() {
            lock (_updateLock) {
                if (_isUpdating)
                    return;
                _isUpdating = true;
            }

            var currentGame = GetCurrentGame();
            if (currentGame != null)
                TryCheckRunning(currentGame);
        }

        void TryCheckRunning(Game game) {
            try {
                var rg = game.Running;
                if (rg == null)
                    return;
                var p = rg.Process;
                if (p == null || !p.SafeHasExited())
                    return;
                _eventBus.PublishOnCurrentThread(new GameTerminated(p));
                game.RegisterTermination();
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            } finally {
                _isUpdating = false;
            }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                _listTimer.Close();
            }
            // free native resources
        }

        #endregion

        #region IHandle events

        public void Handle(ActiveGameChanged message) {
            TryGameVersionCheck(message.Game);
        }

        public void Handle(GameLaunchedEvent message) {
            LastGameLaunch = message.TimeStamp;
            var cgs = message.RunningGame.Game.CalculatedSettings;

            if (cgs.Collection != null &&
                cgs.Collection.Id != Guid.Empty)
                _settings.ModOptions.AddRecent(cgs.Collection);

            if (cgs.Server != null)
                _settings.ServerOptions.AddRecent(cgs.Server);

            if (cgs.Mission != null)
                _settings.MissionOptions.AddRecent(cgs.Mission);
        }

        #endregion
    }

    [DoNotObfuscate]
    public class GameLaunchException : Exception
    {
        public GameLaunchException(string message) : base(message) {}
        public GameLaunchException(string message, Exception exception) : base(message, exception) {}
    }

    [DoNotObfuscate]
    public class JoinServerException : Exception
    {
        public JoinServerException(string fileName, string arguments, string workingDirectory, Exception exception)
            : base(
                "There was an error launching Game.\r\n"
                + "File Name:" + fileName + "\r\n"
                + "Arguments:" + arguments + "\r\n"
                + "Working Directory:" + workingDirectory,
                exception) {}
    }
}