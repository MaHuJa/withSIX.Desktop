// <copyright company="SIX Networks GmbH" file="GamesPreLaunchEventHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    public class GamesPreLaunchEventHandler : IApplicationService
    {
        readonly IDialogManager _dialogManager;
        readonly IronFrontService _ifService;
        readonly IRestarter _restarter;
        readonly UserSettings _settings;
        PreGameLaunchCancelleableEvent _event;

        public GamesPreLaunchEventHandler(IDialogManager dialogManager, UserSettings settings,
            IronFrontService ifService, IRestarter restarter) {
            _dialogManager = dialogManager;
            _settings = settings;
            _ifService = ifService;
            _restarter = restarter;
        }

        // TODO: Async
        public void Process(PreGameLaunchCancelleableEvent message) {
            _event = message;

            if (!message.Cancel)
                message.Cancel = ShouldCancel();

            // TODO: The data to check should already be included, like Executable/Directory, etc?
            // Or even better; include a flag that says if there are already running instances of the game instead?
            if (!message.Cancel)
                CheckRunningProcesses(GetGameExeName());
        }

        bool ShouldCancel() {
            return !ConfirmNoUpdatesAvailable() || !HandleIronFront()
                   || (_event.Server != null && !CheckLaunchServer());
        }

        bool HandleIronFront() {
            return !(_ifService.IsIronFrontEnabled(_event.Collection) && !InstallIronFront());
        }

        bool InstallIronFront() {
            try {
                if (_ifService.IsIronFrontInstalled(DomainEvilGlobal.SelectedGame.ActiveGame))
                    return true;

                if (_dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    @"Please confirm the installation of Iron Front in Arma by clicking OK.

Note: The conversion and patching process will take several minutes - please be patient.",
                    "Iron Front in Arma setup", SixMessageBoxButton.OKCancel)) == SixMessageBoxResult.Cancel)
                    return false;
                _ifService.InstallIronFrontArma(DomainEvilGlobal.SelectedGame.ActiveGame);
            } catch (OaIronfrontNotFoundException) {
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams("OA/IronFront not found?"));
                return false;
            } catch (DestinationDriveFullException ddex) {
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "You have not enough space on the destination drive, you need at least " +
                    Tools.FileUtil.GetFileSize(ddex.RequiredSpace) + " free space on " +
                    ddex.Path));
                return false;
            } catch (TemporaryDriveFullException tdex) {
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "You have not enough space on the temp drive, you need at least " +
                    Tools.FileUtil.GetFileSize(tdex.RequiredSpace) + " free space on " +
                    tdex.Path));
                return false;
            } catch (ElevationRequiredException erex) {
                if (_dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "The IronFront conversion process needs to run as administrator, restarting now, please start the process again after restarted")) ==
                    SixMessageBoxResult.Cancel)
                    return false;
                _restarter.RestartWithUacInclEnvironmentCommandLine();
                return false;
            } catch (UnsupportedIFAVersionException e) {
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "Unsupported Iron Front version. Must either be 1.0, 1.03, 1.04, or 1.05\nFound: " +
                    e.Message));
                return false;
            }
            return true;
        }

        bool ConfirmNoUpdatesAvailable() {
            if (_event.Game.CalculatedSettings.State == OverallUpdateState.Play)
                return true;

            var custom = _event.Collection as CustomCollection;
            if (custom == null || !custom.ForceModUpdate) {
                return
                    _dialogManager.MessageBoxSync(
                        new MessageBoxDialogParams(
                            "There appear to be updates available to install, are you sure you want to launch without installing them?",
                            "Are you sure?", SixMessageBoxButton.YesNo)).IsYes();
            }

            //if (!_event.Game.InstalledState.IsClient)
            //return true;

            _dialogManager.MessageBoxSync(
                new MessageBoxDialogParams(
                    "There appear to be updates available to install, you cannot join this server without installing them (configured by repo admin)"));
            return false;
        }

        bool CheckServerSlots(Server server) {
            if (server.MaxPlayers <= 0 || server.FreeSlots >= _settings.ServerOptions.MinFreeSlots)
                return true;
            var result =
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(String.Format(
                    "The server appears to be at or near capacity, would you like to queue until at least {0} slots are available, or try joining anyway?",
                    _settings.ServerOptions.MinFreeSlots),
                    server.FreeSlots == 0 ? "Server Full" : "Server Near Capacity", SixMessageBoxButton.YesNo) {
                        IgnoreContent = false,
                        GreenContent = "queue for server",
                        RedContent = "join anyway"
                    });
            switch (result) {
            case SixMessageBoxResult.Yes: {
                _event.Game.CalculatedSettings.Queued = server;
                return false;
            }
            case SixMessageBoxResult.Cancel:
                return false;
            }
            return true;
        }

        bool CheckServerPassword(Server server) {
            if (!server.PasswordRequired ||
                (!String.IsNullOrWhiteSpace(server.SavedPassword) && server.SavePassword))
                return true;
            return OpenPasswordDialog(server);
        }

        bool CheckServerVersion(Server server) {
            var gameVer = _event.Game.InstalledState.Version;
            if (server.GameVer == null ||
                server.IsSameGameVersion(gameVer))
                return true;
            return _dialogManager.MessageBoxSync(new MessageBoxDialogParams(String.Format(
                "The server appears to be running a different version of the Game ({0} vs {1}), do you wish to continue?\n\n(You could try 'Force enable beta patch' in the Game Options)",
                server.GameVer, gameVer), "Server appears to run another Game version", SixMessageBoxButton.YesNo))
                .IsYes();
        }

        bool OpenPasswordDialog(Server server) {
            Contract.Requires<ArgumentNullException>(server != null);
            var msg = String.Format("Please enter Server Password for {0}:", server.Name);
            var defaultInput = server.SavedPassword;
            // Hopefully we are on a bg thread here or this fails
            var response = _dialogManager.ShowEnterConfirmDialogAsync(msg, defaultInput).Result;
            if (response.Item1 == SixMessageBoxResult.Cancel)
                return false;

            var input = response.Item2 ?? String.Empty;
            if (response.Item1 == SixMessageBoxResult.YesRemember)
                server.SavePassword = true;
            else {
                server.SavedPassword = null;
                server.SavePassword = false;
            }
            server.SavedPassword = input.Trim();
            return true;
        }

        SixMessageBoxResult ShowPwsUriDialog(Uri pwsUri, string type) {
            return
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "You are trying to join a server that appears to require a " + type +
                        ", would you like to use it?\n"
                        + pwsUri, "Use server custom repository?", SixMessageBoxButton.YesNoCancel));
        }

        bool HandlePwsUriDialogResult(Uri pwsUri, string type) {
            var result = ShowPwsUriDialog(pwsUri, type);
            if (result.IsNo())
                return false;
            if (result != SixMessageBoxResult.Cancel)
                ProcessPwsUri(pwsUri);
            return true;
        }

        static void ProcessPwsUri(Uri pwsUri) {
            Common.App.PublishEvent(new ProcessAppEvent(pwsUri.ToString()));
        }

        bool CheckLaunchServer() {
            if (!CheckServerVersion(_event.Server))
                return false;
            if (!CheckServerPassword(_event.Server))
                return false;
            if (!CheckServerSlots(_event.Server))
                return false;

            if (ShouldProcessPwsCollectionUriOrCancel(_event.Server))
                return false;

            if (ShouldProcessPwsUriOrCancel(_event.Server))
                return false;

            return true;
        }

        bool ShouldProcessPwsUriOrCancel(Server server) {
            var pwsUri = server.GetPwsUriFromName();
            if (pwsUri == null)
                return false;

            return !ActiveModSetMatchesPwsUri(pwsUri) && HandlePwsUriDialogResult(pwsUri, "custom repository");
        }

        bool ShouldProcessPwsCollectionUriOrCancel(Server server) {
            var pwsUri = server.GetPwsCollectionUriFromName();
            if (pwsUri == null)
                return false;

            return !ActiveModSetMatchesPwsCollectionUri(pwsUri) &&
                   HandlePwsUriDialogResult(pwsUri, "shared collection");
        }

        bool ActiveModSetMatchesPwsUri(Uri pwsUri) {
            var customModSet = _event.Collection as CustomCollection;
            return customModSet != null &&
                   (customModSet.CustomRepo != null && customModSet.CustomRepoUrl == pwsUri.ToString());
        }

        bool ActiveModSetMatchesPwsCollectionUri(Uri pwsUri) {
            var getIdFromUrl = Guid.Empty; // TODO
            var customModSet = _event.Collection as CustomCollection;
            if (customModSet != null)
                return customModSet.PublishedId == getIdFromUrl;
            var subscribedModSet = _event.Collection as SubscribedCollection;
            if (subscribedModSet != null)
                return subscribedModSet.CollectionID == getIdFromUrl;

            return false;
        }

        void CheckRunningProcesses(string exeName) {
            var runningProcesses = RunningProcesses(exeName);
            if (runningProcesses.Any())
                ConfirmRunningProcesses(exeName);
        }

        IEnumerable<Process> RunningProcesses(string gameExeName) {
            return String.IsNullOrWhiteSpace(gameExeName)
                ? default(Process[])
                : Tools.Processes.FindProcess(gameExeName);
        }

        string GetGameExeName() {
            return _event.Game.InstalledState.Executable.FileName;
        }

        void ConfirmRunningProcesses(string exeName) {
            if (!_settings.AppOptions.RememberWarnOnGameRunning) {
                var r =
                    _dialogManager.MessageBoxSync(
                        new MessageBoxDialogParams(
                            "Game already appears to be running, would you like to close it before starting another instance?",
                            "Close Game before continuing?", SixMessageBoxButton.YesNo) {RememberedState = false});

                switch (r) {
                case SixMessageBoxResult.YesRemember:
                    _settings.AppOptions.RememberWarnOnGameRunning = true;
                    _settings.AppOptions.WarnOnGameRunning = true;
                    break;
                case SixMessageBoxResult.NoRemember:
                    _settings.AppOptions.RememberWarnOnGameRunning = true;
                    _settings.AppOptions.WarnOnGameRunning = false;
                    break;
                }

                if (r.IsYes())
                    Tools.Processes.KillByName(exeName);
            } else {
                if (_settings.AppOptions.WarnOnGameRunning)
                    Tools.Processes.KillByName(exeName);
            }
        }
    }
}