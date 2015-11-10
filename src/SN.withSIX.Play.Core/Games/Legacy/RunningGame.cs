// <copyright company="SIX Networks GmbH" file="RunningGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Arma.Commands;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class RunningGame : PropertyChangedBase, IDisposable, IEnableLogging
    {
        const int AcMinGameRevision = 100496;
        static readonly ServerAddress defaultServerAddress = new ServerAddress("127.0.0.1:2302");
        readonly CancellationToken _token;
        CancellationTokenSource _cts;
        bool _hosting;
        ServerAddress _hostIP;
        string _message;
        string _playerId;
        Server _server;

        public RunningGame(Game game, Process proc, Collection collection, Server server = null) {
            Game = game;
            Process = proc;
            Collection = collection;
            Server = server;

            _cts = new CancellationTokenSource();
            CommandAPI = new CommandAPI(_cts.Token);
            _token = _cts.Token;

            CommandAPI.MessageReceived.OfType<MissingAddonsMessage>().Subscribe(HandleMissingAddonsMessage);

            var gv = Game.InstalledState.Version;
            if (gv != null && gv.Revision > AcMinGameRevision)
                TryLaunchArmaCommander().ConfigureAwait(false);
        }

        public CommandAPI CommandAPI { get; }
        public Collection Collection { get; private set; }
        public Process Process { get; private set; }
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public Game Game { get; }
        public Server Server
        {
            get { return _server; }
            set { SetProperty(ref _server, value); }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        async Task TryLaunchArmaCommander() {
            try {
                await Task.Delay(8000, _token).ConfigureAwait(false);
                if (!await TryConnect().ConfigureAwait(false))
                    return;
                var loop = CommandAPI.ReadLoop().ConfigureAwait(false);
#if DEBUG
                await CommandAPI.QueueSend(new MessageCommand("Play withSIX connected")).ConfigureAwait(false);
#endif
                this.Logger().Info("AC connected to {0}", Process.Id);
                SetupConnectedTimer();
            } catch (Exception e) {
                this.Logger().FormattedDebugException(e);
            }
        }

        void HandleMissingAddonsMessage(MissingAddonsMessage message) {
            var task = CommandAPI.QueueSend(new MessageCommand("Missing addons detected: " + message.Message));
        }

        async Task<bool> TryConnect() {
            if (!CommandAPI.RetryConnect(15)) {
                this.Logger().Warn("Unable to connect AC");
                return false;
            }

            if (!await CommandAPI.WaitUntilReady().ConfigureAwait(false)) {
                this.Logger().Warn("Unable to get a response from AC");
                return false;
            }
            return true;
        }

        TimerWithElapsedCancellationAsync SetupConnectedTimer() {
            return new TimerWithElapsedCancellationAsync(5000, async () => {
                if (!CommandAPI.IsConnected || !CommandAPI.IsReady)
                    return false;
                try {
                    await ProcessSession().ConfigureAwait(false);
#if DEBUG
                } catch (TimeoutException e) {
                    this.Logger().FormattedDebugException(e);
#else
                } catch (TimeoutException) {
#endif
                } catch (Exception e) {
                    this.Logger().FormattedWarnException(e);
                }
                return true;
            });
        }

        protected virtual async Task ProcessSession() {
            _playerId = String.Empty;
            _hostIP = null;
            _hosting = false;
            var session =
                await
                    CommandAPI.QueueSend<SessionCommand>(new SessionCommand())
                        .ConfigureAwait(false);
            if (session != null)
                ProcessSession(session);
        }

        void ProcessSession(SessionCommand session) {
            var sb = new StringBuilder();
            ProcessHostIp(session);
            _playerId = session.PlayerId;
            if (session.Hosting)
                sb.Append("(HOSTING)");
            Message = sb.ToString();
        }

        void ProcessHostIp(SessionCommand session) {
            var currentHostIp = _hostIP == null ? null : _hostIP.ToString();
            if (session.HostIP == currentHostIp && session.Hosting == _hosting)
                return;

            _hosting = session.Hosting;
            var addr = session.HostIP;
            _hostIP = string.IsNullOrWhiteSpace(addr) ? null : new ServerAddress(addr);

            Common.App.PublishEvent(new MyActiveServerAddressChanged(_hostIP));
        }

        [DoNotObfuscate]
        public async Task CloseGame() {
            if (CommandAPI.IsConnected && CommandAPI.IsReady) {
                if (await CommandAPI.TryQueueSend(new ShutdownCommand()).ConfigureAwait(false) != null)
                    await Task.Delay(10.Seconds(), _token).ConfigureAwait(false);
            }

            TryKillProcess();
        }

        void TryKillProcess() {
            if (Process == null)
                return;

            try {
                Process.Kill();
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        [DoNotObfuscate]
        public void Minimize() {
            TryMinimize();
        }

        void TryMinimize() {
            try {
                var proc = Process;
                if (proc == null || proc.SafeHasExited())
                    return;
                Tools.ProcessesTools.NativeMethods.MinimizeWindow(proc);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        [DoNotObfuscate]
        public void SwitchTo() {
            TrySwitchTo();
        }

        void TrySwitchTo() {
            try {
                var proc = Process;
                if (proc == null || proc.SafeHasExited())
                    return;
                Tools.ProcessesTools.NativeMethods.SetForeground(proc);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                Process.Dispose();
                Process = null;
                CommandAPI.Close();
                //CommandAPI = null;
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            // free native resources
        }
    }
}