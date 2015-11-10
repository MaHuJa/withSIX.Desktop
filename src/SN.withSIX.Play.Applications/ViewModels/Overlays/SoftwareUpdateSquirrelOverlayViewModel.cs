// <copyright company="SIX Networks GmbH" file="SoftwareUpdateSquirrelOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Services;
using Squirrel;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    public interface ISoftwareUpdateSquirrelOverlayViewModel
    {
        ReactiveCommand<Unit> RestartCommand { get; }
        ReactiveCommand<Unit> ApplyUpdateCommand { get; }
        bool NewVersionAvailable { get; }
        bool NewVersionInstalled { get; }
    }

    public class SoftwareUpdateSquirrelOverlayViewModel : SoftwareUpdateOverlayViewModelBase,
        ISoftwareUpdateSquirrelOverlayViewModel
    {
        readonly IRestarter _restarter;
        bool _downloading;
        bool _isApplyingUpdate;
        bool _isCheckingForUpdates;
        bool _newVersionAvailable;
        bool _newVersionInstalled;
        int _progress;
        string _updateStatus;

        public SoftwareUpdateSquirrelOverlayViewModel(SettingsViewModel settingsViewModel, IRestarter restarter)
            : base(settingsViewModel) {
            _restarter = restarter;
            ChangelogUrl = new Uri(CommonUrls.MainUrlHttp, "/changelog/nolayout");

            var observable = this.WhenAnyValue(x => x.IsCheckingForUpdates, x => x.IsApplyingUpdate, (x, y) => !x && !y);

            ReactiveCommand.CreateAsyncTask(observable, x => ApplyUpdate())
                .SetNewCommand(this, x => x.ApplyUpdateCommand);

            ReactiveCommand.CreateAsyncTask(observable, x => SeeIfThereAreUpdates())
                .SetNewCommand(this, x => x.CheckForUpdatesCommand)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(RefreshInfo);

            ApplyUpdateCommand.IsExecuting.BindTo(this, x => x.IsApplyingUpdate);
            CheckForUpdatesCommand.IsExecuting.BindTo(this, x => x.IsCheckingForUpdates);

            ReactiveCommand.CreateAsyncTask(x => Restart())
                .SetNewCommand(this, x => x.RestartCommand);

            this.WhenAnyValue(x => x.IsActive)
                .Where(x => x)
                .Subscribe(x => CheckForUpdatesAtStart());
        }

        public bool IsCheckingForUpdates
        {
            get { return _isCheckingForUpdates; }
            private set { SetProperty(ref _isCheckingForUpdates, value); }
        }
        public bool IsApplyingUpdate
        {
            get { return _isApplyingUpdate; }
            private set { SetProperty(ref _isApplyingUpdate, value); }
        }
        public ReactiveCommand<UpdateInfo> CheckForUpdatesCommand { get; private set; }
        public Uri ChangelogUrl { get; private set; }
        public bool Downloading
        {
            get { return _downloading; }
            set { SetProperty(ref _downloading, value); }
        }
        public string UpdateStatus
        {
            get { return _updateStatus; }
            set { SetProperty(ref _updateStatus, value); }
        }
        public int Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }
        public ReactiveCommand<Unit> ApplyUpdateCommand { get; private set; }
        public ReactiveCommand<Unit> RestartCommand { get; private set; }
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

        async Task Restart() {
            _restarter.RestartInclEnvironmentCommandLine();
        }

        async void CheckForUpdatesAtStart() {
            await TrySeeIfThereAreUpdates().ConfigureAwait(false);
        }

        async Task TrySeeIfThereAreUpdates() {
            try {
                // DO not call ConfigureAwait!
                RefreshInfo(await SeeIfThereAreUpdates());
            } catch (Exception ex) {
                MainLog.Logger.WarnException("Error during update check", ex);
                UpdateStatus = "Error checking for updates";
            }
        }

        Task<UpdateInfo> SeeIfThereAreUpdates() {
            return new SquirrelUpdater().CheckForUpdates();
        }

        void RefreshInfo(UpdateInfo updateInfo) {
            NewVersionAvailable = HasFutureReleaseEntry(updateInfo) && NotEqualVersions(updateInfo);
            UpdateStatus = updateInfo.CurrentlyInstalledVersion.Version + " installed";
            if (NewVersionAvailable) {
                UpdateStatus += ", " + updateInfo.FutureReleaseEntry.Version + " available" +
                                (updateInfo.FutureReleaseEntry.IsDelta ? " (delta)" : "");
            } else
                UpdateStatus += " (latest)";
        }

        static bool HasFutureReleaseEntry(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != null;
        }

        static bool NotEqualVersions(UpdateInfo updateInfo) {
            return updateInfo.FutureReleaseEntry != updateInfo.CurrentlyInstalledVersion;
        }

        async Task ApplyUpdate() {
            try {
                Progress = 0;
                Downloading = true;
                // Do not use configureawait!
                await new SquirrelUpdater().UpdateApp(i => Progress = i);
                NewVersionAvailable = false;
                NewVersionInstalled = true;
                UpdateStatus = "New version installed, feel free to restart at any time";
                //return releaseEntry;
            } finally {
                Downloading = false;
            }
        }
    }
}