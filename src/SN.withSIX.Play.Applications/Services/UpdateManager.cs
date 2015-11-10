// <copyright company="SIX Networks GmbH" file="UpdateManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using MoreLinq;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Applications.ViewModels.Dialogs;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;

namespace SN.withSIX.Play.Applications.Services
{
    public static class StatusExtensions
    {
        public static bool IsEmpty(this InstallStatusOverview overview) {
            Contract.Requires<ArgumentNullException>(overview != null);
            return overview.Collections.IsEmpty() && overview.Mods.IsEmpty() && overview.Missions.IsEmpty();
        }

        public static bool IsEmpty(this InstallStatus status) {
            Contract.Requires<ArgumentNullException>(status != null);
            return !status.Install.Any() && !status.Uninstall.Any() && !status.Update.Any();
        }
    }

    public class UpdateManager : PropertyChangedBase, IUpdateManager,
        IHandle<CalculatedGameSettingsUpdated>,
        IHandle<GameContentInitialSynced>, IHandle<PackageList.CurrentPackageChanged>,
        IHandle<LocalMachineInfoChanged>, IEnableLogging, IDomainService
    {
        static readonly string[] ts3Processes = {"ts3client_win32", "ts3client_win64"};
        readonly IBusyStateHandler _busyStateHandler;
        readonly TimeSpan _checkGameLaunchTimeSpan = new TimeSpan(0, 0, 5);
        readonly TimeSpan _checkGameTimeSpan = new TimeSpan(0, 0, 10);
        readonly TimeSpan _checkTimeSpan = new TimeSpan(0, 0, 30);
        readonly IContentManager _contentManager;
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly LaunchManager _launchManager;
        readonly PathMover _pathMover;
        readonly IRepoActionHandler _repoActionHandler;
        readonly IRestarter _restarter;
        readonly ServerModsMigrator _serverModsMigrator;
        readonly UserSettings _settings;
        readonly Subject<Tuple<Collection, ContentState>> _stateChange;
        ActionStatus _actionState = ActionStatus.Disabled;
        string _actionText = UpdateStates.Initializing;
        string _actionWarningMessage;
        DateTime? _checked;
        DateTime? _checkedGame;
        bool _contentPathsValid;
        Collection _currentCollection;
        Game _currentGame;
        Server _currentServer;
        bool _force;
        bool _gameInstalled;
        bool _isActionEnabled;
        bool _isTerminated;
        ContentState _lastState;
        bool _launching;
        Task _messagePump;
        IList<UpdateState> _modUpdates;
        bool _mp;
        bool _performAction;
        int _progressBarVisiblity;
        bool _refreshInfo;
        Collection _stateCollection;
        bool _supportsMods;
        ContentState _updateState;
        bool _wasSuspended;

        public UpdateManager(IBusyStateHandler busyStateHandler, IRepoActionHandler repoActionHandler,
            IEventAggregator eventBus,
            IContentManager contentManager, LaunchManager launchManager,
            ServerModsMigrator migrator,
            UserSettings settings,
            PathMover pathMover, IDialogManager dialogManager, IRestarter restarter) {
            _eventBus = eventBus;
            _contentManager = contentManager;
            _launchManager = launchManager;
            _settings = settings;
            _pathMover = pathMover;
            _dialogManager = dialogManager;
            _restarter = restarter;
            _serverModsMigrator = migrator;
            _busyStateHandler = busyStateHandler;
            _repoActionHandler = repoActionHandler;
            _repoActionHandler.WhenAnyValue(x => x.ActiveStatusMod)
                .Subscribe(x => OnPropertyChanged("ActiveStatusMod"));
            _repoActionHandler.WhenAnyValue(x => x.ActiveStatusMod.Repo.Info.Progress).Subscribe(SetTaskBarStatus);
            _actionText = UpdateStates.Initializing;

            var isBusyObservable = _busyStateHandler.WhenAnyValue(x => x.IsBusy);
            isBusyObservable.Subscribe(x => SetTaskBarStatus());
            isBusyObservable.Where(x => x)
                .Subscribe(x => { _busyStateHandler.IsAborted = false; });
            isBusyObservable.Where(x => !x)
                .Subscribe(x => { _busyStateHandler.IsAborted = false; });

            _busyStateHandler.WhenAnyValue(x => x.IsSuspended)
                .Where(x => !x)
                .Subscribe(_ => _refreshInfo = true);

            settings.WhenAnyValue(x => x.GameOptions.GameSettingsController.ActiveProfile)
                .Skip(1)
                .Subscribe(_ => _refreshInfo = true);

            _stateChange = new Subject<Tuple<Collection, ContentState>>();
        }

        public bool IsUpdateNeeded
        {
            get { return _updateState != ContentState.Uptodate; }
        }
        public IList<UpdateState> ModUpdates
        {
            get { return _modUpdates; }
            set { SetProperty(ref _modUpdates, value); }
        }

        public void RefreshModInfo() {
            _refreshInfo = true;
        }

        public bool IsActionEnabled
        {
            get { return _isActionEnabled; }
            set
            {
                SetProperty(ref _isActionEnabled, value);
                SetTaskBarStatus();
            }
        }
        public string ActionText
        {
            get { return _actionText; }
            set
            {
                SetProperty(ref _actionText, value);
                SetTaskBarStatus();
            }
        }
        public ActionStatus ActionState
        {
            get { return _actionState; }
            set { SetProperty(ref _actionState, value); }
        }
        public StatusMod ActiveStatusMod
        {
            get { return _repoActionHandler.ActiveStatusMod; }
        }
        public int ProgressBarVisiblity
        {
            get { return _progressBarVisiblity; }
            set { SetProperty(ref _progressBarVisiblity, value); }
        }
        public OverallUpdateState State
        {
            get { return _currentGame.CalculatedSettings.State; }
            set
            {
                var gs = _currentGame;
                if (gs.CalculatedSettings.State == value)
                    return;
                gs.CalculatedSettings.State = value;
                OnPropertyChanged();
            }
        }
        public string ActionWarningMessage
        {
            get { return _actionWarningMessage; }
            set { SetProperty(ref _actionWarningMessage, value); }
        }

        public async Task MoveModFoldersIfValidAndExists(IAbsoluteDirectoryPath sourcePath,
            IAbsoluteDirectoryPath destinationPath) {
            if (sourcePath != null && sourcePath.Exists) {
                using (var statusRepo = new StatusRepo {Action = RepoStatus.Moving}) {
                    await
                        _repoActionHandler.PerformStatusActionWithBusyHandlingAsync(
                            statusRepo,
                            "Migrating mod data",
                            () =>
                                Task.Factory.StartNew(
                                    () => _pathMover.MoveModFolders(sourcePath, destinationPath),
                                    TaskCreationOptions.LongRunning)).ConfigureAwait(false);
                }
            }
        }

        public async Task MovePathIfValidAndExists(IAbsoluteDirectoryPath sourcePath,
            IAbsoluteDirectoryPath destinationPath) {
            if (sourcePath != null && sourcePath.Exists
                && destinationPath != null && !destinationPath.Exists) {
                using (var statusRepo = new StatusRepo {Action = RepoStatus.Moving}) {
                    await
                        _repoActionHandler.PerformStatusActionWithBusyHandlingAsync(
                            statusRepo,
                            "Migrating synq data",
                            () =>
                                Task.Factory.StartNew(
                                    () =>
                                        Tools.FileUtil.Ops.MoveDirectory(sourcePath, destinationPath),
                                    TaskCreationOptions.LongRunning)).ConfigureAwait(false);
                }
            }
        }

        public Task ProcessRepoApps() {
            var customModSet = _currentCollection as CustomCollection;
            return customModSet != null ? ProcessServerApps(customModSet) : TaskExt.Default;
        }

        public async Task ProcessRepoMpMissions() {
            var customModSet = _currentCollection as CustomCollection;
            if (customModSet == null)
                return;
            if (customModSet.CustomRepo.Config.MPMissions.Any()) {
                await
                    _repoActionHandler.PerformUpdaterActionSuspendedAsync("Fetch Repo MPMissions",
                        () => FetchRepoMpMissions(customModSet)).ConfigureAwait(false);
            }
        }

        public async Task ProcessRepoMissions() {
            var customModSet = _currentCollection as CustomCollection;
            if (customModSet == null)
                return;
            if (customModSet.CustomRepo.Config.Missions.Any()) {
                await
                    _repoActionHandler.PerformUpdaterActionSuspendedAsync("Fetch Repo Missions",
                        () => FetchRepoMissions(customModSet)).ConfigureAwait(false);
            }
        }

        public Task Play() {
            _currentGame.CalculatedSettings.Queued = null;
            return _launchManager.StartGame();
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task HandleConvertOrInstallOrUpdate(bool force = false) {
            using (_busyStateHandler.StartSession())
                await DoIt(force).ConfigureAwait(false);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public Task HandleUninstall() {
            return _repoActionHandler.PerformUpdaterActionSuspendedAsync("Uninstall", OnHandleUninstall);
        }

        public void Terminate() {
            _isTerminated = true;
            Abort();
        }

        [SmartAssembly.Attributes.ReportUsage]
        public Task DownloadMission(Mission mission) {
            return _repoActionHandler.PerformUpdaterActionSuspendedAsync("Install mission",
                () => HandleMissionDownloadAndInstall(mission));
        }

        public IObservable<Tuple<Collection, ContentState>> StateChange
        {
            get { return _stateChange.AsObservable(); }
        }

        async Task<Tuple<LicenseResult, string>> LicenseDialog(IEnumerable<LicenseInfo> mods, string modSetName) {
            var licenseDialogViewModel = new LicenseDialogViewModel(mods, modSetName);

            if (licenseDialogViewModel.DialogResult != LicenseResult.LicensesError)
                await _dialogManager.ShowDialogAsync(licenseDialogViewModel);

            var licensesFailed = licenseDialogViewModel.DialogResult ==
                                 LicenseResult.LicensesError
                ? licenseDialogViewModel.LicensesFailed
                : "";

            return new Tuple<LicenseResult, string>(licenseDialogViewModel.DialogResult, licensesFailed);
        }

        async Task DoIt(bool force) {
            using (_busyStateHandler.StartSuspendedSession())
                await OnHandleConvertOrInstallOrUpdate(force).ConfigureAwait(false);
        }

        async Task HandleCustomRepoLaunch() {
            var customModSet = _currentCollection as CustomCollection;
            if (customModSet == null || customModSet.CustomRepo == null)
                return;

            if (_currentGame.InstalledState.IsClient)
                await TryProcessApps().ConfigureAwait(false);

            await TryProcessMpMissions().ConfigureAwait(false);
        }

        async Task<bool> TryProcessApps() {
            try {
                await ProcessCustomRepoApps().ConfigureAwait(false);
                return true;
            } catch (Exception e) {
                await
                    UserError.Throw(new InformationalUserError(e, "An error occurred during processing of Server Apps",
                        null));
                return false;
            }
        }

        async Task<bool> TryProcessMpMissions() {
            try {
                var customModSet = _currentCollection as CustomCollection;
                if (customModSet == null || customModSet.CustomRepo == null ||
                    !customModSet.CustomRepo.Config.MPMissions.Any())
                    return false;
                await FetchRepoMpMissions(customModSet).ConfigureAwait(false);
                return true;
            } catch (Exception e) {
                await
                    UserError.Throw(new InformationalUserError(e, "An error occurred during processing of MPMissions",
                        null));
            }
            return false;
        }

        async Task ProcessCustomRepoApps() {
            var modSet = _currentCollection as CustomCollection;
            if (modSet == null)
                return;
            var apps = modSet.CustomRepoApps;
            if (apps == null || !apps.Any())
                return;

            UsageCounter.ReportUsage(String.Format("Dialog - Auto process server apps: {0}",
                _settings.ServerOptions.AutoProcessServerApps));
            if (_settings.ServerOptions.AutoProcessServerApps
                ||
                (await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                    "The server appears to have apps to process, would you like to?",
                    "Process server apps?", SixMessageBoxButton.YesNo))).IsYes())
                await ProcessServerApps(modSet).ConfigureAwait(false);
        }

        static async Task ProcessServerApps(IHaveCustomRepo modSet) {
            foreach (var app in modSet.CustomRepoApps) {
                switch (app.Type) {
                case AppType.Teamspeak3: {
                    if (KillTeamspeak3Processes())
                        await Task.Delay(2000);
                    Tools.Generic.TryOpenUrl(app.Address);
                    break;
                }
                }
            }
        }

        static bool KillTeamspeak3Processes(bool gracefully = true) {
            return ts3Processes.Select(x => Tools.Processes.KillByName(x, null, gracefully)).ToArray()
                .Any();
        }

        Task FetchRepoMpMissions(IHaveCustomRepo modSet) {
            var gamePath = _currentGame.InstalledState.Directory;
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(
                gamePath.GetChildDirectoryWithName("MPMissions"));
            return _repoActionHandler.PerformStatusActionAsync("Fetch MPMissions",
                repo => modSet.CustomRepo.DownloadMPMissions(gamePath, repo));
        }

        Task FetchRepoMissions(IHaveCustomRepo modSet) {
            var gamePath = _currentGame.InstalledState.Directory;
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(
                gamePath.GetChildDirectoryWithName("Missions"));
            return _repoActionHandler.PerformStatusActionAsync("Fetch Missions",
                repo => modSet.CustomRepo.DownloadMissions(gamePath, repo));
        }

        void Abort() {
            this.Logger().Info("Abort Update");
            _busyStateHandler.IsAborted = true;
            var mod = _repoActionHandler.ActiveStatusMod;
            if (mod != null)
                mod.Abort();

            // TODO: Verify this is now automatic!
            //TryKillTransferProcesses();
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void AbortUpdate() {
            if (!_settings.AppOptions.RememberWarnOnAbortUpdate) {
                var r =
                    _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                        "Would you like to abort the download/update process?",
                        "About to abort", SixMessageBoxButton.YesNo) {RememberedState = false});

                switch (r) {
                case SixMessageBoxResult.YesRemember:
                    UsageCounter.ReportUsage("Dialog - Remember Warn On Abort Update");
                    _settings.AppOptions.RememberWarnOnAbortUpdate = true;
                    _settings.AppOptions.WarnOnAbortUpdate = true;
                    break;
                case SixMessageBoxResult.NoRemember:
                    UsageCounter.ReportUsage("Dialog - Don't remember Warn On Abort Update");
                    _settings.AppOptions.RememberWarnOnAbortUpdate = true;
                    _settings.AppOptions.WarnOnAbortUpdate = false;
                    break;
                }

                if (r.IsYes())
                    Abort();
            } else {
                if (_settings.AppOptions.WarnOnAbortUpdate)
                    Abort();
            }
        }

        bool HandleLaunch() {
            if (_launchManager.LastGameLaunch != null &&
                !Tools.Generic.LongerAgoThan(_launchManager.LastGameLaunch.Value, _checkGameLaunchTimeSpan)) {
                SetMainButtonState(UpdateStates.Launching, false);
                _launching = true;
                return true;
            }
            if (!_launching)
                return false;

            _launching = false;
            _currentServer = null;

            return false;
        }

        void SetTaskBarStatus(double value = -1) {
            if (!_busyStateHandler.IsBusy && IsActionEnabled) {
                ProgressBarVisiblity = 0;
                return;
            }

            var progress = value > 0.0 ? value : GetStatusProgress();
            if (progress < 1.0 || progress >= 100.0)
                ProgressBarVisiblity = 3;
            else
                ProgressBarVisiblity = 1;
        }

        double GetStatusProgress() {
            var asm = _repoActionHandler.ActiveStatusMod;
            return asm == null ? 0.0 : asm.Repo.Info.Progress;
        }

        IEnumerable<ModController> GetModControllers() {
            return _currentGame.CalculatedSettings.CurrentMods
                .Select(x => x.Controller)
                .Where(x => x != null);
        }

        void TryProcessModAppsAndUserconfig(ModStates states) {
            try {
                ProcessModAppsAndUserconfig(states);
            } catch (OperationCanceledException) {
                this.Logger().Info("User cancelled the userconfigs tasks");
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Failure during processing of userconfigs", null));
            }
        }

        void ProcessModAppsAndUserconfig(ModStates states) {
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(
                _currentGame.InstalledState.Directory.GetChildDirectoryWithName("Userconfig"));
            foreach (var mod in states.Procced.Where(x => x.Exists))
                mod.TryProcessModAppsAndUserconfig(_currentGame, states.Userconfigs.Contains(mod));
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void MainAction() {
            IsActionEnabled = false;
            if (State == OverallUpdateState.NoGameFound)
                _eventBus.PublishOnUIThread(new RequestGameSettingsOverlay(_currentGame.Id));
            else
                _performAction = true;
        }

        void SetMainButtonState(string text, bool state,
            ActionStatus actionStatus = ActionStatus.Disabled, string actionWaringMessage = null) {
            ActionWarningMessage = actionWaringMessage;
            ActionText = text;
            IsActionEnabled = state;
            ActionState = actionStatus;
        }

        ContentState UpdatesNeeded() {
            if (!_gameInstalled)
                return ContentState.Uptodate;

            var mission = _currentGame.CalculatedSettings.Mission;
            var currentMods = GetModControllers().Select(x => x.Mod).ToArray();
            UpdateModStates(currentMods);
            var supportedChangedMods =
                currentMods.Where(x => x.State != ContentState.Uptodate).Where(x => !(x is LocalMod))
                    .Select(x => x.Controller)
                    .Cast<ContentController>().ToArray();

            var updates = GetMissionUpdates(mission).Concat(supportedChangedMods).ToArray();
            ModUpdates = updates
                .Select(x => x.CreateUpdateState())
                .OrderBy(x => x.Mod.Name)
                .ToArray();

            if (!updates.Any())
                return ContentState.Uptodate;

            if (updates.Any(x => x.Model.State == ContentState.NotInstalled))
                return ContentState.NotInstalled;

            return updates.Any(x => x.Model.State == ContentState.Unverified)
                ? ContentState.Unverified
                : ContentState.UpdateAvailable;
        }

        void UpdateModStates(IReadOnlyCollection<IMod> currentMods) {
            // TODO: Integrate ServerMods to ModSets and let them be processed by ModSet.UpdateState()
            // TODO: Consider never working without a modset, so even "No ModSet" is actually a modset? that would solve a lot of null checks and what not
            var modding = _currentGame as ISupportModding;
            if (modding != null)
                modding.UpdateModStates(currentMods);

            if (_currentCollection != null)
                _currentCollection.UpdateState();
            _contentManager.UpdateCollectionStates();
        }

        static IEnumerable<ContentController> GetMissionUpdates(MissionBase mission) {
            return GetMissionUpdRequired(mission)
                ? new[] {mission.Controller}
                : Enumerable.Empty<ContentController>();
        }

        static bool GetMissionUpdRequired(MissionBase mission) {
            if (mission == null)
                return false;
            var controller = mission.Controller;
            return controller.IsUpdateRequired();
        }

        async Task HandleMissionDownloadAndInstall(MissionBase mission) {
            if (!HandleMissionPreRequisites())
                return;
            SetMainButtonState(UpdateStates.Updating, false);
            _currentGame.CalculatedSettings.Mission = mission;
            await ProcessMissionPackages(() => DownloadAndInstallMissionPackage(mission)).ConfigureAwait(false);
        }

        bool HandleMissionPreRequisites() {
            return CheckPaths();
        }

        Task ProcessMissionPackages(Func<Task> act) {
            return _repoActionHandler.PerformStatusActionAsync(GetSynqMessage("mission", 1),
                statusRepo => ProcessMissionPackage(act, statusRepo));
        }

        Task ProcessMissionPackage(Func<Task> act, StatusRepo statusRepo) {
            return ProcessPackage(_currentGame.Controller, act, statusRepo);
        }

        async Task DownloadAndInstallMissionPackage(MissionBase mission) {
            var gm = _currentGame.Controller;

            var controller = mission.Controller;
            var actualDependency = controller.Package.ActualDependency;

            var packages =
                await
                    gm.PackageManager.ProcessPackage(actualDependency, false, true,
                        !DomainEvilGlobal.Settings.AppOptions.KeepCompressedFiles).ConfigureAwait(false);
            foreach (var package in packages)
                await ProcessMissionPackage(mission, package).ConfigureAwait(false);
        }

        async Task ProcessMissionPackage(MissionBase mission, Package package) {
            var ct = package.MetaData.ContentType;
            if (!string.IsNullOrWhiteSpace(ct) && ct.EndsWith("Mission")) {
                var missionPath = _currentGame.InstalledState.Directory.GetChildDirectoryWithName(mission.PathType());
                Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(missionPath);
                package.SetWorkingPath(missionPath.ToString());
            }

            // TODO: Check if we actually need to checkout the data?
            // We can probably move this into above condition statement, as mods are already handled through MissionMods?
            await package.CheckoutWithoutRemovalAsync().ConfigureAwait(false);
        }

        void TryKillTransferProcesses() {
            this.Logger().Info("Killing remaining rsync/zsync processes...");

            var pid = Process.GetCurrentProcess().Id;
            TryKillChildren(pid, "zsync.exe");
            TryKillChildren(pid, "rsync.exe");
        }

        void TryKillChildren(int pid, string exe) {
            try {
                Tools.Processes.KillNamedProcessChildren(exe, pid);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e, "Error during killing of processes: " + exe);
            }
        }

        async Task<bool> CheckModPathUac() {
            var modding = _currentGame as ISupportModding;
            if (modding == null)
                return true;
            return !await _restarter.CheckUac(modding.ModPaths.Path).ConfigureAwait(false);
        }

        async Task OnHandleConvertOrInstallOrUpdate(bool force = false) {
            _force = force;

            if (!await HandleModPreRequisites().ConfigureAwait(false))
                return;

            _busyStateHandler.IsAborted = false;
            UpdateMainButton();

            await PerformConvertOrInstallOrUpdate().ConfigureAwait(false);

            if (_busyStateHandler.IsAborted && !_isTerminated) {
                UsageCounter.ReportUsage("Dialog - Update process aborted by user");
                await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("Update process aborted by user"));
            }
        }

        async Task PerformConvertOrInstallOrUpdate() {
            Exception e = null;
            try {
                await TryConvertOrInstallOrUpdate().ConfigureAwait(false);
            } catch (Exception ex) {
                e = ex;
            }
            await TempProcessing(UpdateCurrentModSet);
            _currentGame.RefreshState();
            _currentGame.CalculatedSettings.UpdateSignatures();
            if (e != null)
                throw e;
        }

        Task TryConvertOrInstallOrUpdate() {
            return _repoActionHandler.TryUpdaterActionAsync(ConvertOrInstallOrUpdate, ActionText ?? "Diagnosing");
        }

        async Task<bool> HandleModPreRequisites() {
            UpdateCurrentInfoWhenChanged();
            if (!CheckPaths() || !await CheckModPathUac().ConfigureAwait(false))
                return false;

            await _currentGame.Controller.AdditionalHandleModPreRequisites().ConfigureAwait(false);
            return true;
        }

        void UpdateCurrentInfoWhenChanged() {
            if (!IsCurrentModSetChanged())
                return;

            _currentCollection = _currentGame.CalculatedSettings.Collection;
            Reset();
        }

        bool CheckPaths() {
            var invalidPath = _currentGame.InvalidPaths();
            if (invalidPath == null)
                return true;

            _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                String.Format(
                    "The following path appears to be invalid: {0}\nValue: {1}\nPlease configure in the Game Settings.\n\nTo use this software you must own and have fully installed a supported game.",
                    invalidPath.Item2, invalidPath.Item1)));
            return false;
        }

        async Task OnHandleUninstall() {
            if (!await HandleModPreRequisites().ConfigureAwait(false))
                return;

            await
                Task.Factory.StartNew(Uninstall, TaskCreationOptions.LongRunning)
                    .ConfigureAwait(false);
        }

        void Uninstall() {
            var mods = DomainEvilGlobal.SelectedGame.ActiveGame.CalculatedSettings.Collection.EnabledMods;
            foreach (var mm in mods.Select(mod => mod.Controller).Where(x => x.Exists))
                Tools.FileUtil.Ops.AddIORetryDialog(() => mm.Uninstall(), mm.Path.ToString());
            _refreshInfo = true;
        }

        void UpdateMainButton() {
            SetMainButtonState(GetTask() + "..", false);
        }

        string GetTask() {
            switch (_updateState) {
            case ContentState.Uptodate:
                return UpdateStates.Checking;
            case ContentState.Unverified:
                return UpdateStates.Checking;
            case ContentState.NotInstalled:
                return UpdateStates.Installing;
            default:
                return UpdateStates.Updating;
            }
        }

        Task StartMessagePump() {
            return _messagePump = Task.Factory.StartNew(() => Run(), TaskCreationOptions.LongRunning).Unwrap();
        }

        async Task Run() {
            while (true) {
                await InitializeRun().ConfigureAwait(false);
                await RunUntilTerminatedOrModSetChange().ConfigureAwait(false);
                if (_isTerminated)
                    break;
            }
        }

        async Task InitializeRun() {
            while (_busyStateHandler.IsBusy)
                Thread.Sleep(200);

            SetMainButtonState(UpdateStates.ProcessState, false);
            _currentGame = DomainEvilGlobal.SelectedGame.ActiveGame;
            _supportsMods = _currentGame.SupportsMods();
            _currentCollection = _currentGame.CalculatedSettings.Collection;
            await UpdateCurrentModSet().ConfigureAwait(false);
            LogModInfo();
            Reset();
        }

        async Task RunUntilTerminatedOrModSetChange() {
            while (!_isTerminated && !GameOrModSetChanged()) {
                await ProcessState().ConfigureAwait(false);
                await Task.Delay(200).ConfigureAwait(false);
            }
        }

        void LogModInfo() {
            string modName;
            if (_currentCollection != null) {
                var firstMod = _currentCollection.GetFirstMod();
                modName = firstMod != null ? firstMod.Name : "vanilla";
            } else
                modName = "vanilla";

            this.Logger()
                .Info("Activating {0} ({1}), Thread: {2}", _currentCollection == null ? "None" : _currentCollection.Name,
                    modName, Thread.CurrentThread.ManagedThreadId);
        }

        async Task ProcessState() {
            HandleCheckGame();

            if (_gameInstalled && (!_supportsMods || _contentPathsValid))
                await ProcessValidState().ConfigureAwait(false);
            else
                await ProcessInvalidState().ConfigureAwait(false);

            TryHandleAppsButton();
        }

        void HandleCheckGame() {
            var checkedGame = _checkedGame;
            if (checkedGame == null ||
                Tools.Generic.LongerAgoThan(checkedGame.Value, _checkGameTimeSpan))
                CheckGameInstalled();
        }

        async Task ProcessInvalidState() {
            SetMainButtonState(!_gameInstalled ? UpdateStates.GameNotFound : UpdateStates.InvalidPaths, true,
                ActionStatus.NoGameFound,
                "(click to locate game)");
            State = OverallUpdateState.NoGameFound;
            await HandleGameInfoChecked(false).ConfigureAwait(false);
            ModUpdates = new List<UpdateState>();
        }

        async Task ProcessValidState() {
            if (State == OverallUpdateState.NoGameFound) {
                State = OverallUpdateState.QuickPlay;
                _checked = null;
            }

            HandleServer();

            var mp = _currentServer != null;
            var switchedMode = mp ? !_mp : _mp;
            _mp = mp;

            await HandleGameInfoChecked(switchedMode).ConfigureAwait(false);

            if (_busyStateHandler.IsSuspended)
                HandleSuspended();
            else
                await HandleUnsuspended(switchedMode).ConfigureAwait(false);
        }

        void HandleServer() {
            var server = _currentGame.CalculatedSettings.Server;
            if (server == _currentServer)
                return;

            if (server != null)
                server.TryUpdateAsync().ConfigureAwait(false);
            _currentServer = server;
        }

        async Task HandleGameInfoChecked(bool switchedMode) {
            var @checked = _checked;
            if (@checked == null || switchedMode || _refreshInfo ||
                Tools.Generic.LongerAgoThan(@checked.Value, _checkTimeSpan))
                await RefreshGameInfo().ConfigureAwait(false);
        }

        async Task HandleUnsuspended(bool switchedMode) {
            if (_wasSuspended) {
                IsActionEnabled = true;
                _wasSuspended = false;
            }
            if (switchedMode) {
                _currentServer = null;
                _currentGame.CalculatedSettings.Queued = null;
            }
            await HandleModeAction().ConfigureAwait(false);
        }

        async Task HandleModeAction() {
            if (_mp)
                RunMp();
            else
                RunSp();
            await HandleAction().ConfigureAwait(false);
        }

        void HandleSuspended() {
            var state = IsActionEnabled;
            if (!_wasSuspended)
                _wasSuspended = state;
            IsActionEnabled = false;
        }

        void CheckGameInstalled() {
            _gameInstalled = _currentGame.InstalledState.IsInstalled;
            var modding = _currentGame as ISupportModding;
            var missions = _currentGame as ISupportMissions;
            var moddingValid = modding == null || modding.ModPaths.IsValid;
            var missionValid = missions == null || missions.MissionPaths.All(x => x.IsValid);
            _contentPathsValid = moddingValid && missionValid;
            _checkedGame = Tools.Generic.GetCurrentUtcDateTime;
        }

        Task UpdateCurrentModSet() {
            return _contentManager.RefreshCollectionInfo(_currentCollection);
        }

        async Task TempProcessing(Func<Task> action) {
            var currentText = ActionText;
            var currentEnabled = IsActionEnabled;
            var currentActionState = ActionState;
            var currentWarning = ActionWarningMessage;
            SetMainButtonState(UpdateStates.ProcessState, false);
            try {
                await action().ConfigureAwait(false);
            } finally {
                SetMainButtonState(currentText, currentEnabled, currentActionState, currentWarning);
            }
        }

        async Task RefreshGameInfo() {
            if (GameOrModSetChanged())
                return;

            CheckGameInstalled();

            await _currentGame.CalculatedSettings.HandleSubGames().ConfigureAwait(false);
            _updateState = UpdatesNeeded();
            _currentGame.UpdateStartupLine();

            _refreshInfo = false;
            _checked = Tools.Generic.GetCurrentUtcDateTime;
        }

        bool GameOrModSetChanged() {
            return IsActiveGameSetChanged() || IsCurrentModSetChanged();
        }

        bool IsCurrentModSetChanged() {
            return _currentCollection != _currentGame.CalculatedSettings.Collection;
        }

        bool IsActiveGameSetChanged() {
            return _currentGame != DomainEvilGlobal.SelectedGame.ActiveGame;
        }

        void Reset() {
            State = OverallUpdateState.QuickPlay;
            _currentServer = null;
            _performAction = false;
            _checked = null;
            _checkedGame = null;
            _updateState = ContentState.Uptodate;
            _refreshInfo = false;
        }

        async Task<int> ConvertOrInstallOrUpdateSixSyncMod(ModController mod) {
            var tries = 0;
            while (!_busyStateHandler.IsAborted && tries < 2) {
                tries++;
                try {
                    await
                        _repoActionHandler.PerformStatusActionAsync(Path.GetFileName(mod.Mod.Name),
                            statusRepo =>
                                mod.ConvertOrInstallOrUpdateSixSync(tries > 1, statusRepo,
                                    _currentGame.Modding()
                                        .ModPaths.RepositoryPath.GetChildDirectoryWithName(
                                            Repository.DefaultRepoRootDirectory).GetChildDirectoryWithName("legacy")
                                        .GetChildDirectoryWithName(".pack"))).ConfigureAwait(false);
                } catch (HostListExhausted) {
                    if (tries >= 2)
                        throw;
                    this.Logger().Warn("Failed first time, trying again, with additional fallback");
                }
            }

            return 0;
        }

        async Task ConvertOrInstallOrUpdate() {
            await MakeSureAllRepositoriesExist().ConfigureAwait(false);
            await MigrateServerMods().ConfigureAwait(false);

            var mods = GetModControllers().Where(x => !(x.Mod is LocalMod)).ToArray();
            var mission = _currentGame.CalculatedSettings.Mission as Mission;
            var modArray = mods.Select(x => x.Mod).ToArray();

            await ConfirmLicenses(modArray);

            var packagedMods = mods.Where(x => x.Package != null && x.Package.ActualDependency != null).ToArray();

            var modStates = new ModStates();


            if (await CIUPackageMods(packagedMods, modStates).ConfigureAwait(false)) {
                await ProcessInstallState(modStates.InstallState).ConfigureAwait(false);
                return;
            }

            var sixSyncMods = mods.Except(packagedMods).ToArray();
            if (await CIUSixSyncMods(sixSyncMods, modStates).ConfigureAwait(false)) {
                await ProcessInstallState(modStates.InstallState).ConfigureAwait(false);
                return;
            }

            await ProcessInstallState(modStates.InstallState).ConfigureAwait(false);

            if (_busyStateHandler.IsAborted)
                return;

            TryPostInstallPreLaunch(modStates.Procced);
            TryProcessModAppsAndUserconfig(modStates);
            await ProcessCustomRepoMPMissions().ConfigureAwait(false);
            await ProcessMissionPackage(mission).ConfigureAwait(false);
        }

        async Task ProcessInstallState(InstallStatusOverview state) {
            if (state.IsEmpty())
                return;
            await Tools.Transfer.PostJson(state, new Uri(CommonUrls.SocialApiUrl, "/api/stats")).ConfigureAwait(false);
        }

        void TryPostInstallPreLaunch(IReadOnlyCollection<ModController> procced, bool launch = false) {
            try {
                _currentGame.PostInstallPreLaunch(procced, launch);
            } catch (OperationCanceledException ex) {
                this.Logger().Info("User cancelled the postinstall/prelaunch tasks");
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Failure during processing of postinstall/prelaunch tasks",
                    null));
            }
        }

        async Task MakeSureAllRepositoriesExist() {
            var gameControllers = GetValidGameControllers();
            foreach (var c in gameControllers)
                await c.UpdateBundleManager().ConfigureAwait(false);
        }

        IEnumerable<GameController> GetValidGameControllers() {
            return new[] {_currentGame}.Where(x => x.InstalledState.IsInstalled)
                .Select(x => x.Controller);
        }

        async Task MigrateServerMods() {
            if (!await _serverModsMigrator.ShouldMoveServerMods(_currentGame))
                return;

            _repoActionHandler.PerformStatusAction("Moving servermods",
                statusRepo => _serverModsMigrator.MigrateServerMods(_currentGame, statusRepo));
        }

        async Task<bool> CIUSixSyncMods(IEnumerable<ModController> sixSyncMods, ModStates states) {
            var sixSyncModsToProcess = sixSyncMods.Where(mod => {
                mod.UpdateState();
                var needUpdate = mod.IsInstalled && mod.Model.State != ContentState.Uptodate;
                return _force || !mod.IsInstalled || needUpdate;
            }).ToArray();

            if (CheckFreespace(sixSyncModsToProcess))
                return true;

            await ProcessSixSyncMods(sixSyncModsToProcess, states).ConfigureAwait(false);
            sixSyncModsToProcess.ForEach(x => x.UpdateState());
            return false;
        }

        async Task<bool> CIUPackageMods(ModController[] packagedMods, ModStates states) {
            var packageModsToProcess = packagedMods.Where(mod => {
                mod.UpdateState();
                var needUpdate = mod.IsInstalled && mod.Model.State != ContentState.Uptodate;
                return _force || !mod.IsInstalled || needUpdate;
            }).ToArray();

            if (CheckFreespace(packageModsToProcess))
                return true;

            await ProcessPackages(packageModsToProcess, states).ConfigureAwait(false);
            CleanupOldDirectories(packagedMods);
            packageModsToProcess.ForEach(x => x.UpdateState());
            return false;
        }

        async Task ProcessCustomRepoMPMissions() {
            var customModSet = _currentCollection as CustomCollection;
            if (customModSet == null || customModSet.CustomRepo == null)
                return;

            await
                TryProcessMpMissions().ConfigureAwait(false);
        }

        async Task ProcessMissionPackage(MissionBase mission) {
            if (mission != null && !mission.IsLocal)
                await ProcessMissionPackages(() => DownloadAndInstallMissionPackage(mission)).ConfigureAwait(false);
        }

        void CleanupOldDirectories(IEnumerable<ModController> packagedMods) {
            foreach (var rsyncDir in GetRsyncDirs(packagedMods))
                DeleteRsyncDir(rsyncDir);
        }

        async Task ProcessSixSyncMods(ModController[] sixSyncMods, ModStates states) {
            var gamePath = _currentGame.InstalledState.Directory;
            var modPath = _currentGame.Modding().ModPaths.Path;
            var nonExistent =
                sixSyncMods.Select(x => x.Mod.Name).Where(x => !modPath.GetChildDirectoryWithName(x).Exists);
            var existInGame = nonExistent.Where(x => gamePath.GetChildDirectoryWithName(x).Exists).ToArray();

            if (!HandleExistingGameMods(existInGame, gamePath, modPath))
                return;

            foreach (var mm in sixSyncMods) {
                if (_busyStateHandler.IsAborted)
                    break;
                await ProcessSixSyncMod(mm, states.Userconfigs).ConfigureAwait(false);
                ProcessModInstallState(states.InstallState, mm);
                states.Procced.Add(mm);
            }
        }

        static void ProcessModInstallState(InstallStatusOverview state, ModController mm) {
            // Only official network mods
            if (mm.Mod is CustomRepoMod || mm.Mod.State == ContentState.Local)
                return;

            // TODO: This is dangerous if the Mod states are refreshed before the process finishes....
            switch (mm.Mod.State) {
            case ContentState.NotInstalled:
                state.Mods.Install.Add(mm.Mod.Id);
                break;
            case ContentState.UpdateAvailable:
                state.Mods.Update.Add(mm.Mod.Id);
                break;
            }
        }

        async Task ProcessPackages(ModController[] toProcess, ModStates states, bool useVersioned = false) {
            var dic = GetDependencyDictionary(toProcess);
            if (!HandleMigrations(useVersioned, dic))
                return;
            await HandlePackages(dic).ConfigureAwait(false);
            foreach (var mm in toProcess)
                ProcessModInstallState(states.InstallState, mm);
            states.Userconfigs.AddRange(toProcess);
            states.Procced.AddRange(toProcess);
        }

        async Task HandlePackages(Dictionary<GameController, List<SpecificVersion>> dic) {
            foreach (var i in dic)
                await ProcessModPackages(i.Key, i.Value).ConfigureAwait(false);
        }

        bool HandleMigrations(bool useVersioned, Dictionary<GameController, List<SpecificVersion>> dic) {
            foreach (var set in dic) {
                if (!ProcessGameMigrations(useVersioned, set))
                    return false;
            }
            return true;
        }

        bool ProcessGameMigrations(bool useVersioned,
            KeyValuePair<GameController, List<SpecificVersion>> set) {
            var modding = set.Key.Game as ISupportModding;
            if (modding == null)
                return true;

            var gamePath = set.Key.Game.InstalledState.Directory;
            var modPath = modding.ModPaths.Path;

            var names = set.Value.Select(x => useVersioned ? x.GetFullName() : x.Name).ToArray();

            if (gamePath != null) {
                var nonExistent = names.Where(x => !modPath.GetChildDirectoryWithName(x).Exists);
                var existInGame = nonExistent.Where(x => gamePath.GetChildDirectoryWithName(x).Exists).ToArray();

                if (!HandleExistingGameMods(existInGame, gamePath, modPath))
                    return false;
            }

            return true;
        }

        Dictionary<GameController, List<SpecificVersion>> GetDependencyDictionary(
            IEnumerable<ModController> toProcess) {
            var dic = new Dictionary<GameController, List<SpecificVersion>>();
            foreach (var mod in toProcess)
                UpdateGameDictionary(mod, dic);
            return dic;
        }

        void UpdateGameDictionary(ModController mod,
            IDictionary<GameController, List<SpecificVersion>> dic) {
            var gm = mod.Game.Controller;
            if (!dic.ContainsKey(gm))
                dic[gm] = new List<SpecificVersion>();
            dic[gm].Add(mod.Package.ActualDependency);
        }

        bool HandleExistingGameMods(string[] existInGame, IAbsoluteDirectoryPath gamePath,
            IAbsoluteDirectoryPath modPath) {
            if (!existInGame.Any())
                return true;

            var result =
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "The following mods exist in Game directory, but not in the mod directory, do you want to copy or move them, or cancel?\nThis can take a while, please be patient once made choice\n\n"
                    + string.Join(", ", existInGame),
                    "Existing mods found in game folder, copy, move or cancel.", SixMessageBoxButton.YesNoCancel) {
                        IgnoreContent = false,
                        GreenContent = "copy",
                        BlueContent = "cancel",
                        RedContent = "move"
                    });
            switch (result) {
            case SixMessageBoxResult.YesRemember: {
                CopyMods(existInGame, gamePath, modPath);
                break;
            }
            case SixMessageBoxResult.Yes: {
                CopyMods(existInGame, gamePath, modPath);
                break;
            }
            case SixMessageBoxResult.NoRemember: {
                MoveMods(existInGame, gamePath, modPath);
                break;
            }
            case SixMessageBoxResult.No: {
                MoveMods(existInGame, gamePath, modPath);
                break;
            }
            default:
                return false;
            }
            return true;
        }

        void MoveMods(IEnumerable<string> existInGame, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath modPath) {
            _repoActionHandler.PerformStatusAction("Move existing",
                statusRepo => MoveMods(existInGame, gamePath, modPath, statusRepo));
        }

        static void MoveMods(IEnumerable<string> existInGame, IAbsoluteDirectoryPath gamePath,
            IAbsoluteDirectoryPath modPath, StatusRepo statusRepo) {
            statusRepo.Action = RepoStatus.Moving;
            foreach (var mod in existInGame) {
                var status = new Status(mod, statusRepo) {Action = RepoStatus.Processing};
                Tools.FileUtil.Ops.MoveDirectoryWithUpdaterFallbackAndRetry(gamePath.GetChildDirectoryWithName(mod),
                    modPath.GetChildDirectoryWithName(mod));
                status.Progress = 100;
            }
        }

        void CopyMods(IEnumerable<string> existInGame, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath modPath) {
            _repoActionHandler.PerformStatusAction("Copy existing",
                statusRepo => CopyMods(existInGame, gamePath, modPath, statusRepo));
        }

        static void CopyMods(IEnumerable<string> existInGame, IAbsoluteDirectoryPath gamePath,
            IAbsoluteDirectoryPath modPath, StatusRepo statusRepo) {
            statusRepo.Action = RepoStatus.Copying;
            foreach (var mod in existInGame) {
                var status = new Status(mod, statusRepo) {Action = RepoStatus.Processing};
                Tools.FileUtil.Ops.CopyDirectoryWithUpdaterFallbackAndRetry(gamePath.GetChildDirectoryWithName(mod),
                    modPath.GetChildDirectoryWithName(mod));
                status.Progress = 100;
            }
        }

        async Task ConfirmLicenses(IEnumerable<IMod> mods) {
            var neededLicenses = LicenseDialogNeeded(mods).ToArray();
            if (!neededLicenses.Any())
                return;

            if (_currentGame.InstalledState.IsClient)
                await HandleClientResult(neededLicenses);
            else {
                // Auto accept on servers/headless clients... hmm
                _settings.ModOptions.AddAcceptedLicenses(neededLicenses);
            }
        }

        async Task HandleClientResult(IMod[] neededLicenses) {
            var modSet = _currentCollection;
            var result = await LicenseDialog(
                neededLicenses.Select(
                    x => new LicenseInfo {Title = x.Name, Id = x.Id, Version = x.ModVersion}),
                modSet == null ? "" : modSet.Name);
            switch (result.Item1) {
            case LicenseResult.LicensesAccepted:
                _settings.ModOptions.AddAcceptedLicenses(neededLicenses);
                break;
            case LicenseResult.LicensesDeclined:
                throw new UserDeclinedLicenseException("Mod license(s) not accepted by user. Install aborted.");
            case LicenseResult.LicensesError:
                throw new LicenseRetrievalException("Mod license retrieval failed for the following mod(s):\n" +
                                                    result.Item2);
            }
        }

        Task ProcessModPackages(GameController gm, IEnumerable<SpecificVersion> packages) {
            return _repoActionHandler.PerformStatusActionAsync(GetSynqMessage("mod", packages.Count()),
                statusRepo => ProcessModPackage(gm, packages, statusRepo));
        }

        static Task ProcessModPackage(GameController gm, IEnumerable<SpecificVersion> packages,
            StatusRepo statusRepo) {
            return ProcessPackage(gm, () => gm.PackageManager.ProcessPackages(packages), statusRepo);
        }

        static string GetSynqMessage(string type, int count) {
            return String.Format("{0} {1}", count, type.PluralizeIfNeeded(count));
        }

        static async Task ProcessPackage(GameController gm, Func<Task> act, StatusRepo statusRepo) {
            gm.PackageManager.StatusRepo = statusRepo;
            try {
                await act().ConfigureAwait(false);
            } catch (AllZsyncFailException e) {
                WarnAboutAllZsyncFail(e);
            }
        }

        void DeleteRsyncDir(IAbsoluteDirectoryPath rsyncDir) {
            try {
                var repo =
                    Sync.Core.Legacy.SixSync.Repository.Factory.Open(Path.GetDirectoryName(rsyncDir.DirectoryName));
                Tools.FileUtil.Ops.DeleteDirectory(repo.PackFolder);
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
            Tools.FileUtil.Ops.DeleteDirectory(rsyncDir);
        }

        static IEnumerable<IAbsoluteDirectoryPath> GetRsyncDirs(IEnumerable<ModController> packagedMods) {
            return packagedMods
                .Select(x => x.Path.GetChildDirectoryWithName(Sync.Core.Legacy.SixSync.Repository.RepoFolderName))
                .Where(x => x.Exists);
        }

        async Task ProcessSixSyncMod(ModController mm, List<ModController> userconfigs) {
            var userconfig = !mm.IsInstalled;

            try {
                await ConvertOrInstallOrUpdateSixSyncMod(mm).ConfigureAwait(false);
            } catch (AllZsyncFailException e) {
                WarnAboutAllZsyncFail(e);
            }

            if (userconfig)
                userconfigs.Add(mm);
        }

        bool CheckFreespace(ModController[] toProcess) {
            var size = toProcess.Sum(x => x.Mod.Size);
            var wdsize = toProcess.Sum(x => x.Mod.SizeWd);
            var mp = _currentGame.Modding().ModPaths.Path;
            if (mp == null)
                return false;
            var drive = DriveInfo.GetDrives()
                .FirstOrDefault(d => mp.ToString().ToLower().StartsWith(d.Name.ToLower()));
            if (drive == null)
                return false;

            if (drive.AvailableFreeSpace >= size + wdsize)
                return false;
            UsageCounter.ReportUsage("Dialog - Not enough free space to install mods");
            if (!_dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                "There is not enough free space on the drive to install your mods, are you sure you want to continue?",
                "Not enough free space", SixMessageBoxButton.YesNo)).IsNo())
                return false;
            _dialogManager.MessageBoxSync(new MessageBoxDialogParams("Not enough free space.",
                "User Aborted Operation",
                SixMessageBoxButton.OK));
            return true;
        }

        static void WarnAboutAllZsyncFail(AllZsyncFailException e) {
            UserError.Throw(new InformationalUserError(e,
                "This exception, if your connection is otherwise working, is usually due to interference from AV software.\n" +
                "Try to resolve it by disabling AV or excluding the Zsync binaries.", "All Zsync mirrors failed."));
        }

        IEnumerable<IMod> LicenseDialogNeeded(IEnumerable<IMod> mods) {
            return
                mods.Where(
                    x =>
                        (!(x is CustomRepoMod)) && x.HasLicense &&
                        !_settings.ModOptions.AcceptedLicenseUUIDs.Contains(x.Id));
        }

        [SmartAssembly.Attributes.ReportUsage]
        void HandlePlay(bool mp = false) {
            TryHandlePlay(mp ? UpdateStates.JoinServerState : UpdateStates.LaunchGameState);
        }

        void TryHandlePlay(string type) {
            try {
                SetMainButtonState(type, false);
                Play();
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Unhandled exception during Play action: " + e.Message,
                    null));
            } finally {
                SetMainButtonState(
                    String.Format("{0}{1}", type, _currentGame.CalculatedSettings.Queued != null ? " (Queued)" : null),
                    true,
                    ActionStatus.Play);
            }
        }

        async Task HandleAction() {
            var performAction = _performAction;
            _performAction = false;

            if (!performAction) {
                await ProcessQueued().ConfigureAwait(false);
                return;
            }

            try {
                await HandleActionState().ConfigureAwait(false);
                // TODO: This should not be allowed here - this isn't the app/VM layer!
            } catch (BusyStateHandler.BusyException) {
                UsageCounter.ReportUsage("Dialog - Cannot perform action; already busy");
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                    "Cannot perform action; already busy.\nPlease wait until current action has completed",
                    "Warning, cannot perform action while already busy"));
            }
        }

        async Task HandleActionState() {
            switch (State) {
            case OverallUpdateState.QuickPlay: {
                break;
            }
            case OverallUpdateState.Update: {
                _checked = null;
                await HandleConvertOrInstallOrUpdate().ConfigureAwait(false);
                break;
            }
            case OverallUpdateState.Play: {
                _currentGame.CalculatedSettings.Queued = null;
                HandlePlay(_currentServer != null);
                break;
            }
            case OverallUpdateState.NoGameFound: {
                _eventBus.PublishOnUIThread(new RequestGameSettingsOverlay(_currentGame.Id));
                break;
            }
            }
        }

        async Task ProcessQueued() {
            var queued = _currentGame.CalculatedSettings.Queued;
            if (queued == null)
                return;
            var slots = queued.FreeSlots;
            var minFreeSlots = _settings.ServerOptions.MinFreeSlots;
            if (slots <= minFreeSlots && (minFreeSlots <= 0 || slots != minFreeSlots))
                return;

            Common.App.PublishDomainEvent(new QueuedServerReadyEvent(queued));

            _currentGame.CalculatedSettings.Server = queued;
            await Play().ConfigureAwait(false);
            _currentGame.CalculatedSettings.Queued = null;
        }

        void RunSp() {
            if (HandleLaunch())
                return;

            if (_updateState != ContentState.Uptodate)
                HandleSpUpdateNeeded();
            else
                HandleSpUpdateNotNeeded();
        }

        void HandleSpUpdateNotNeeded() {
            SetMainButtonState(UpdateStates.LaunchGameState, true, ActionStatus.Play);
            State = OverallUpdateState.Play;
        }

        void HandleSpUpdateNeeded() {
            var task = GetChangeTask();
            HandleModFreespaceState(task);
            State = OverallUpdateState.Update;
        }

        string GetChangeTask() {
            if (_updateState == ContentState.Unverified)
                return UpdateStates.DiagnoseState;
            return _updateState == ContentState.NotInstalled
                ? UpdateStates.InstallState
                : UpdateStates.UpdateState;
        }

        void TryHandleAppsButton() {
            if (_stateCollection == _currentCollection && _lastState == _updateState)
                return;
            try {
                _stateChange.OnNext(Tuple.Create(_currentCollection, _updateState));
            } finally {
                _stateCollection = _currentCollection;
                _lastState = _updateState;
            }
        }

        void RunMp() {
            if (HandleLaunch())
                return;

            if (_updateState != ContentState.Uptodate)
                HandleMpUpdateNeeded();
            else
                HandleMpNoUpdateNeeded();
        }

        void HandleMpNoUpdateNeeded() {
            if (_currentServer == null)
                SetMainButtonState(UpdateStates.SelectServer, false);
            else {
                SetMainButtonState(GetJoinServerStatus(), true, ActionStatus.Play);
                State = OverallUpdateState.Play;
            }
        }

        string GetJoinServerStatus() {
            return UpdateStates.JoinServerState +
                   String.Format("{0}", _currentGame.CalculatedSettings.Queued != null ? " (Queued)" : null);
        }

        void HandleMpUpdateNeeded() {
            HandleModFreespaceState(GetChangeTask());
            State = OverallUpdateState.Update;
        }

        void HandleModFreespaceState(string task) {
            var freeSpaceWarning = false;
            if (_supportsMods) {
                var desiredDrive = _currentGame.Modding().ModPaths.Path.ToString().ToLower();
                var modDrive = DriveInfo.GetDrives()
                    .FirstOrDefault(drive => desiredDrive.StartsWith(drive.Name.ToLower()));
                freeSpaceWarning = modDrive != null && (float) SafeFreeSpace(modDrive)/modDrive.TotalSize < 0.1;
            }
            SetMainButtonState(task, true, ActionStatus.Update, freeSpaceWarning ? "<10% Free Space" : null);
        }

        static long SafeFreeSpace(DriveInfo driveInfo) {
            try {
                return driveInfo.AvailableFreeSpace;
            } catch (IOException ex) {
                MainLog.Logger.WarnException("Error while trying to confirm free space for " + driveInfo.Name, ex);
                return 0;
            }
        }

        class ModStates
        {
            public readonly InstallStatusOverview InstallState = new InstallStatusOverview {
                Missions =
                    new InstallStatus {
                        Install = new List<Guid>(),
                        Uninstall = new List<Guid>(),
                        Update = new List<Guid>()
                    },
                Mods =
                    new InstallStatus {
                        Install = new List<Guid>(),
                        Uninstall = new List<Guid>(),
                        Update = new List<Guid>()
                    },
                Collections =
                    new InstallStatus {
                        Install = new List<Guid>(),
                        Uninstall = new List<Guid>(),
                        Update = new List<Guid>()
                    }
            };
            public readonly List<ModController> Procced = new List<ModController>();
            public readonly List<ModController> Userconfigs = new List<ModController>();
        }

        public static class UpdateStates
        {
            public const string LaunchGameState = "Launch";
            public const string UpdateState = "Update";
            public const string InstallState = "Install";
            public const string DiagnoseState = "Diagnose";
            public const string JoinServerState = "Join";
            public const string Syncing = "Syncing..";
            public const string Initializing = "Initializing...";
            public const string ProcessState = "Processing..";
            public const string Launching = "Launching...";
            public const string Updating = "Updating";
            public const string Refreshing = "Refreshing";
            public const string GameNotFound = "Game not found";
            public const string InvalidPaths = "Invalid paths";
            public const string Installing = "Installing";
            public const string Checking = "Checking";
            public const string SelectServer = "Select server";
        }

        #region IHandle events

        public void Handle(CalculatedGameSettingsUpdated message) {
            _checkedGame = null;
            _checked = null;
        }

        public void Handle(PackageList.CurrentPackageChanged message) {
            _refreshInfo = true;
        }

        public async void Handle(GameContentInitialSynced message) {
            DomainEvilGlobal.SelectedGame.ActiveGame.RefreshState();
            await StartMessagePump().ConfigureAwait(false);
        }

        public void Handle(LocalMachineInfoChanged message) {
            DomainEvilGlobal.SelectedGame.ActiveGame.RefreshState();
        }

        public async Task PreGameLaunch() {
            var modControllers = GetModControllers().Where(x => x.Exists).ToArray();
            var modStates = new ModStates();
            modStates.Procced.AddRange(modControllers);
            TryProcessModAppsAndUserconfig(modStates);
            await HandleCustomRepoLaunch().ConfigureAwait(false);
            TryPostInstallPreLaunch(modControllers, true);
        }

        #endregion
    }

    public class QueuedServerReadyEvent : IDomainEvent
    {
        public QueuedServerReadyEvent(Server queued) {
            Queued = queued;
        }

        public Server Queued { get; private set; }
    }
}