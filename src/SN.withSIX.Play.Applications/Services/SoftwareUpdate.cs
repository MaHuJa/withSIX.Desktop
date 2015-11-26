// <copyright company="SIX Networks GmbH" file="SoftwareUpdate.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Options;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;

namespace SN.withSIX.Play.Applications.Services
{
    public class SoftwareUpdate : PropertyChangedBase, IHandle<NoNewVersionAvailable>,
        IHandle<NewVersionAvailable>, IHandle<NewVersionDownloaded>,
        IHandle<CheckingForNewVersion>, ISoftwareUpdate, IDisposable, IApplicationService
    {
        readonly IDialogManager _dialogManager;
        readonly UserSettings _settings;
        readonly IShutdownHandler _shutdownHandler;
        readonly object _updateLock = new Object();
        Uri _changelogURL;
        bool _newVersionAvailable;
        bool _newVersionDownloaded;
        bool _newVersionInstalled;
        TimerWithElapsedCancellationOnExceptionOnly _timer;
        string _updateStatus;

        public SoftwareUpdate(ISelfUpdater su, UserSettings settings, IDialogManager dialogManager,
            IShutdownHandler shutdownHandler) {
            _dialogManager = dialogManager;
            _shutdownHandler = shutdownHandler;
            _settings = settings;
            SU = su;
            _changelogURL = new Uri(CommonUrls.MainUrl, "/changelog/nolayout");
            CurrentVersion = Common.App.ApplicationVersion;
            HandleIsNewVersionInstalled();
            IsNotInstalled = Common.Flags.SelfUpdateSupported && !SU.IsInstalled();
        }

        public Version CurrentVersion { get; }
        public bool IsNotInstalled { get; }
        public ISelfUpdater SU { get; }
        public Uri ChangelogURL
        {
            get { return _changelogURL; }
            set { SetProperty(ref _changelogURL, value); }
        }
        public bool NewVersionAvailable
        {
            get { return _newVersionAvailable; }
            set { SetProperty(ref _newVersionAvailable, value); }
        }
        public bool NewVersionInstalled
        {
            get { return _newVersionInstalled; }
            set { SetProperty(ref _newVersionInstalled, value); }
        }
        public bool NewVersionDownloaded
        {
            get { return _newVersionDownloaded; }
            set { SetProperty(ref _newVersionDownloaded, value); }
        }
        public string UpdateStatus
        {
            get { return _updateStatus; }
            set { SetProperty(ref _updateStatus, value); }
        }

        public bool UpdateAndExitIfNotBusy(bool force = false) {
            if (!SU.ExistsAndIsValid(Common.Paths.EntryLocation))
                return false;

            if (Common.AppCommon.IsBusy) {
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "A software update has been downloaded (v" + (SU.RemoteVersion ?? String.Empty) +
                    ") but the application appears to be busy.\nIf you would like to install the update after finished, please restart the application.\n" +
                    SU.Destination,
                    "Software Update available"));
                return false;
            }

            if (!_settings.AppOptions.ShowDialogWhenUpdateDownloaded)
                return force && TryRunUpdaterIfValid(Common.Flags.FullStartupParameters);

            var r =
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "A software update has been downloaded (v" + (SU.RemoteVersion ?? String.Empty) +
                        "), would you like to restart to install it?\n" + SU.Destination,
                        "Software Update available", SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "update and restart",
                            RedContent = "cancel"
                        });
            return r.IsYes() && TryRunUpdaterIfValid(Common.Flags.FullStartupParameters);
        }

        public bool InstallAndExitIfNotBusy() {
            if (!InstallIfNotBusy())
                return false;
            _shutdownHandler.Shutdown();
            return true;
        }

        public async Task TryCheckForUpdates() {
            if (SU.NewVersionDownloaded || SU.IsRunning || !Common.Flags.SelfUpdateSupported)
                return;

            await SU.CheckForUpdate().ConfigureAwait(false);
        }

        public Version OldVersion { get; set; }

        bool UpdateAndExitIfNeeded() {
            if (!SU.ExistsAndIsValid(Common.Paths.EntryLocation))
                return false;

            TryRunUpdaterIfValid(Common.Flags.FullStartupParameters);
            return true;
        }

        void OnElapsed() {
            TryCheckForUpdates().WaitAndUnwrapException();
        }

        void HandleIsNewVersionInstalled() {
            if (_settings.OldVersion == null) {
                NewVersionInstalled = false;
                return;
            }
            OldVersion = _settings.OldVersion;
            NewVersionInstalled = _settings.AppOptions.Initialized != 0;
        }

        bool TryRunUpdaterIfValid(params string[] startupParams) {
            try {
                if (!SU.ExistsAndIsValid(Common.Paths.EntryLocation))
                    return false;

                if (Common.Flags.LockDown)
                    SU.PerformSelfUpdate(SelfUpdaterCommands.UpdateCommand, GetLockDownParameter());
                else
                    SU.PerformSelfUpdate(SelfUpdaterCommands.UpdateCommand, startupParams);

                _shutdownHandler.Shutdown();
                return true;
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e,
                    "An error occurred while trying to initiate self-update. See log file for details.\nPlease try again later...",
                    null));
                return false;
            }
        }

        static string GetLockDownParameter() {
            return string.Format("pws://?mod_set={0}&lockdown=true", Common.Flags.LockDownModSet);
        }

        string GetUpdateStatus(Version latestVersion) {
            if (CurrentVersion == latestVersion)
                return "Running latest available " + latestVersion;
            if (CurrentVersion < latestVersion)
                return "Not running latest available " + latestVersion + " (current " + CurrentVersion + ")";
            return "Running newer version " + CurrentVersion + " (latest " + latestVersion + ")";
        }

        static Version TryParseVersion(string ver) {
            Version version;
            Version.TryParse(ver.OrEmpty(), out version);
            return version;
        }

        bool AutoInstallAndExitIfNotBusy() {
            if (!AutoInstallIfNotBusy())
                return false;
            _shutdownHandler.Shutdown();
            return true;
        }

        bool TryRunInstallerIfValid(params string[] startupParams) {
            try {
                if (Common.Flags.LockDown)
                    SU.PerformSelfUpdate(SelfUpdaterCommands.InstallCommand, GetLockDownParameter());
                else
                    SU.PerformSelfUpdate(SelfUpdaterCommands.InstallCommand, startupParams);
                _shutdownHandler.Shutdown();
                return true;
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e,
                    "An error occurred while trying to initiate self-update. See log file for details.\nPlease try again later...",
                    null));
                return false;
            }
        }

        bool AutoInstallIfNotBusy() {
            if (!BusyCheck())
                return false;

            var r =
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "You already have an older version of Play withSIX installed. This setup will update and overwrite the previous installation.\nYou can always do this manually anytime from the check for updates screen.",
                        "Update Play withSIX from 1.3 to " + Common.App.ApplicationVersion + "?",
                        SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "install",
                            RedContent = "abort and don't ask again"
                        });
            if (r.IsYes())
                return TryRunInstallerIfValid(Common.Flags.FullStartupParameters);
            if (r.IsNo())
                _settings.AppOptions.DenyAutoInstall = true;
            return false;
        }

        bool BusyCheck() {
            if (!Common.AppCommon.IsBusy)
                return true;
            _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                "Cannot install currently, application appears busy." +
                SU.Destination,
                "Software Update available"));
            return false;
        }

        bool InstallIfNotBusy() {
            if (!BusyCheck())
                return false;

            var r =
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "Would you like to continue launching Play withSIX from the downloaded .exe file or would you like us to install Play withSIX to your program files and create shortcut files for you?",
                        "Install Play withSIX?", SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "restart and install",
                            RedContent = "cancel"
                        });
            return r.IsYes() && TryRunInstallerIfValid(Common.Flags.FullStartupParameters);
        }

        async Task UpdateInfo() {
            if (Common.Flags.AutoUpdateEnabled && !SU.ExistsAndIsValid(Common.Paths.EntryLocation))
                await TryCheckForUpdates().ConfigureAwait(false);
        }

        #region IDisposable

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool b) {
            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }
        }

        #endregion

        #region IHandle events

        public void Handle(CheckingForNewVersion message) {
            UpdateStatus = "Checking for updates...";
        }

        public void Handle(NewVersionAvailable message) {
            NewVersionAvailable = true;
            UpdateStatus = message.Version + " available, downloading..";
        }

        public void Handle(NewVersionDownloaded message) {
            UpdateStatus = message.Version + " downloaded, ready to install..";
            NewVersionDownloaded = true;
        }

        public void Handle(NoNewVersionAvailable message) {
            if (message.Version == null)
                UpdateStatus = "Failure during version check";
            UpdateStatus = GetUpdateStatus(message.Version);
        }

        #endregion
    }
}