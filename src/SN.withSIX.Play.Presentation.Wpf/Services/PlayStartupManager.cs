// <copyright company="SIX Networks GmbH" file="PlayStartupManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using ReactiveUI;
using ShortBus;
using SimpleInjector;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Wpf;
using SN.withSIX.Core.Presentation.Wpf.Services;
using SN.withSIX.Play.Applications;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Api;
using SN.withSIX.Play.Infra.Server;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    public class PlayStartupManager : WpfStartupManager, IPlayStartupManager
    {
        const string ExMessage = "Issue occurred during software update check";
        readonly ContactList _contactList;
        readonly Container _container;
        readonly IContentManager _contentManager;
        readonly IDialogManager _dialogManager;
        readonly ContentEngine.Infra.ContentEngine _dummyForSA;
        readonly IFirstTimeLicense _firstTimeLicense;
        readonly Cache.ImageFileCache _imageFileCache;
        readonly IPreRequisitesInstaller _prerequisitesInstaller;
        readonly UserSettings _settings;
        readonly IShutdownHandler _shutdownHandler;
        readonly ISoftwareUpdate _softwareUpdate;
        readonly IUserSettingsStorage _storage;
        readonly ISystemInfo _systemInfo;
        readonly Lazy<IUpdateManager> _updateManager;
        readonly VersionRegistry _versionRegistry;
        bool _firstCheck = true;

        public PlayStartupManager(UserSettings settings, IShutdownHandler shutdownHandler,
            IFirstTimeLicense firstTimeLicense, ISystemInfo systemInfo, IUserSettingsStorage storage,
            ISoftwareUpdate softwareUpdate, Container container, ICacheManager cacheManager,
            Cache.ImageFileCache imageFileCache,
            IPreRequisitesInstaller prerequisitesInstaller, ContactList contactList,
            IContentManager contentManager, IDialogManager dialogManager,
            VersionRegistry versionRegistry, Lazy<IUpdateManager> updateManager)
            : base(systemInfo, cacheManager, dialogManager) {
            _settings = settings;
            _shutdownHandler = shutdownHandler;
            _firstTimeLicense = firstTimeLicense;
            _softwareUpdate = softwareUpdate;
            _container = container;
            _imageFileCache = imageFileCache;
            _prerequisitesInstaller = prerequisitesInstaller;
            _contactList = contactList;
            _contentManager = contentManager;
            _dialogManager = dialogManager;
            _versionRegistry = versionRegistry;
            _updateManager = updateManager;
            _systemInfo = systemInfo;
            _storage = storage;
        }

        public virtual void FirstTimeLicenseDialog(object obj) {
            if (_settings.AppOptions.FirstTimeLicenceAccepted)
                return;
            if (!_firstTimeLicense.ConfirmLicense(obj)) {
                _shutdownHandler.Shutdown();
                return;
            }
            _settings.AppOptions.FirstTimeLicenceAccepted = true;
        }

        public async Task VisualInit() {
            using (this.Bench()) {
                _settings.AppOptions.WhenAnyValue(x => x.ParticipateInCustomerExperienceProgram)
                    .Skip(1)
                    .Where(x => !x)
                    .Subscribe(
                        x =>
                            _dialogManager.MessageBoxSync(
                                new MessageBoxDialogParams(
                                    "Please reconsider enabling the Customer Experience Program, it helps us improve the performance, reliability and functionality, anonymously.")));


                await OnlineInit().ConfigureAwait(false);

                if (_softwareUpdate.NewVersionInstalled && _softwareUpdate.OldVersion != null &&
                    _softwareUpdate.OldVersion.Major == 1 && _softwareUpdate.OldVersion.Minor < 5) {
                    await _dialogManager.MessageBoxAsync(
                        new MessageBoxDialogParams(
                            "Hi and welcome to this new version of Play withSIX!\n\nBefore you can Play or Update mods, the mods will need to be converted to the new Synq Package format.\nDon't worry, no or very little data will be redownloaded.\n\nThe process starts for each mod the first time you will use it.",
                            "Welcome to the new Play withSIX!"));
                }

                /*                _settings.AppOptions.WhenAnyValue(x => x.UseElevatedService)
                    .Subscribe(
                        x => _prerequisitesInstaller.SharedInstaller.HandleElevatedService(x).ConfigureAwait(false));*/

                _settings.AppOptions.WhenAnyValue(x => x.LaunchWithWindows)
                    .Subscribe(SetupLaunchWithWindows);
            }
        }

        public string GetSecurityWarning() {
            string securityStatus = null;

            if ((_systemInfo.DidDetectAVRun) && (_systemInfo.IsAVDetected)) {
                var av = new List<string>();
                if (_systemInfo.InstalledAV.Any())
                    av.Add("AV: " + String.Join(", ", _systemInfo.InstalledAV));
                if (_systemInfo.InstalledFW.Any())
                    av.Add("FW: " + String.Join(", ", _systemInfo.InstalledFW));
                var avStr = String.Join("\n", av);
                securityStatus = !string.IsNullOrWhiteSpace(avStr)
                    ? avStr + "\n\nPlease make sure all Play withSIX and SIX Tools executables are allowed or excluded"
                    : null;
            } else if (!_systemInfo.DidDetectAVRun) {
                securityStatus = "Scanning of installed security suites failed" +
                                 "\n\nIf you experience problems, please make sure all Play withSIX and SIX Tools executables are allowed or excluded in any security products you have installed";
            }

            return securityStatus;
        }

        public void HandleSoftwareUpdate() {
            var oldVersion = _settings.AppVersion;
            _settings.AppVersion = CommonBase.AssemblyLoader.GetEntryVersion();
            if (oldVersion != _settings.AppVersion)
                _settings.OldVersion = oldVersion;
        }

        public void ClearAwesomiumCache() {
            ClearAwesomiumWorkaround();
            if (_settings.OldVersion == null)
                return;

            Common.AppCommon.ClearAwesomiumCache();
        }

        public void StartAwesomium() {
            SixAwesomiumStart.Initialize(_settings);
        }

        public Task LaunchSignalr() {
            // TODO: Drop container reference... (just import a service that deals with it?)
            return Task.Run(() => {
                try {
                    var server = new StartThisServer().Start(_container.GetInstance<IMediator>(),
                        _container.GetInstance<IDependencyResolver>());
                } catch (Exception ex) {
                    // TODO: Use IExceptionHandler etc
                    MainLog.Logger.FormattedErrorException(ex, "Error during start of local server");
                }
            });
        }

        public override void RegisterServices() {
            base.RegisterServices();

            Cache.ImageFiles = _imageFileCache;
        }

/*
        public void 3() {
            if (!string.IsNullOrWhiteSpace(_settings.AccountOptions.AccessToken))
                UsageCounter.ReportUsage("Play: Registered");
        }
*/

        public void RegisterUrlHandlers() {
            foreach (var protocol in SixRepo.URLSchemes)
                RegisterProtocol(protocol);
        }

        void ClearAwesomiumWorkaround() {
            if (_settings.ClearedAwesomium)
                return;
            _settings.ClearedAwesomium = true;
            Common.AppCommon.ClearAwesomium();
        }

        static void SetupLaunchWithWindows(bool launchWithWindows) {
            string appName = Common.AppCommon.ApplicationName;
            const string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";

            using (var key = Registry.CurrentUser.OpenSubKey(keyName, true)) {
                if (key == null)
                    return;
                if (launchWithWindows)
                    key.SetValue(appName, Common.Paths.EntryLocation);
                else
                    key.DeleteValue(appName, false);
            }
        }

        async Task InitialSync() {
            _updateManager.Value.ActionText = UpdateManager.UpdateStates.Syncing;
            await _contentManager.InitAsync(_systemInfo.IsInternetAvailable).ConfigureAwait(false);
            await CheckLoadedSyncData();
        }

        async Task OnlineInit() {
            await UiTaskHandler.TryAction(() => _versionRegistry.Init(),
                "Processing of Version info").ConfigureAwait(false);

            await _prerequisitesInstaller.InstallPreRequisites().ConfigureAwait(false);

            InitConnect();

            RunSelfUpdateInTheBackground();

            if (_systemInfo.IsInternetAvailable) {
                await
                    UiTaskHandler.TryAction(() => CheckDeprecatedVersion(), "Deprecated version check")
                        .ConfigureAwait(false);
            }

            await InitialSync().ConfigureAwait(false);
        }

        void RunSelfUpdateInTheBackground() {
            // TODO: Better check based on DEVENV??
#if !DEBUG
                SquirrelSelfUpdate();
#endif
        }

        async void SquirrelSelfUpdate() {
            await Task.Run(() => CheckForUpdatesLoop()).ConfigureAwait(false);
        }

        async Task CheckForUpdatesLoop() {
            while (true) {
                if (_systemInfo.IsInternetAvailable)
                    await CheckForUpdates().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromMinutes(60)).ConfigureAwait(false);
            }
        }

        Task CheckForUpdates() {
            if (!_firstCheck)
                return CheckForNewVersionHandleException();
            _firstCheck = false;
            return UiTaskHandler.TryAction(() => CheckForNewVersion(), ExMessage);
        }

        static async Task CheckForNewVersionHandleException() {
            try {
                await CheckForNewVersion().ConfigureAwait(false);
            } catch (Exception ex) {
                MainLog.Logger.FormattedWarnException(ex, ExMessage);
            }
        }

        static async Task CheckForNewVersion() {
            var newVersion = await new PlaySquirrel().GetNewVersion().ConfigureAwait(false);
            if (newVersion != null)
                Common.App.PublishDomainEvent(new NewVersionAvailable(newVersion));
        }

        async void InitConnect() {
            await _contactList.HandleConnection().ConfigureAwait(false);
        }

        async Task CheckDeprecatedVersion() {
            if (_versionRegistry.VersionInfo.Play.DeprecatedVersion == null) {
                this.Logger().Warn("DeprecatedVersion null, cannot verify version!");
                return;
            }

            if (Common.App.ApplicationVersion > _versionRegistry.VersionInfo.Play.DeprecatedVersion)
                return;

            const string downloadURL = "http://withsix.com/download";

            var r =
                await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        "Your version of Play withSIX is unsupported because it is very outdated. We highly recommend you upgrade to the newest version now.\n" +
                        String.Format("The newest version can always be found at {0}", downloadURL),
                        "You are running an unsupported version of PwS", SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "go there now",
                            RedContent = "cancel"
                        });

            UsageCounter.ReportUsage("Dialog - Upgrade outdated version PwS: {0}".FormatWith(r));

            if (r.IsYes())
                Tools.Generic.TryOpenUrl(downloadURL);
        }

        async Task CheckLoadedSyncData() {
            if (!_contentManager.SyncManagerSynced) {
                await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams("Could not load online Sync data or any local cached data.\n" +
                                               "You will need to go online at least once to populate the 'Play withSIX' application with data and make it useful.\n" +
                                               "Once you have a local data cache you can use the application offline, although we recommend staying online to ensure you receive the newest content.\n\n" +
                                               "The Windows network icon right down below on the tray notification area should indicate connection to the internet,\n" +
                                               "If it does, please confirm any Firewall or other security suite allows Play withSIX to connect to the internet and try again",
                        "Loading of all Sync data failed."));
            }
        }

        static void RegisterProtocol(string protocol) {
            var key = Registry.CurrentUser.CreateSubKey(Path.Combine(Common.AppCommon.Classes, protocol));
            key.SetValue(String.Empty, "URL:Play withSIX " + protocol + " Protocol");
            key.SetValue("URL Protocol", String.Empty);

            var iconKey = key.CreateSubKey("DefaultIcon");
            iconKey.SetValue(String.Empty, Common.Paths.EntryLocation + ",1");

            key = key.CreateSubKey(@"shell\open\command");
            key.SetValue(String.Empty, Common.Paths.EntryLocation.EscapePath() + " \"%1\"");
        }

        public void UnregisterUrlHandlers() {
            Registry.CurrentUser.DeleteSubKey(Common.AppCommon.Classes);
        }

        protected override async Task ExitI() {
            _settings.AppOptions.Initialized = Common.AppCommon.InitializationLvl;
            // Storage saves also a DB so we cant close things beforehand...
            await _storage.SaveNow().ConfigureAwait(false);

            await base.ExitI().ConfigureAwait(false);
        }
    }

    public interface IPreRequisitesInstaller
    {
        Task InstallPreRequisites();
    }

    public class PreRequisitesInstaller : IApplicationService, IPreRequisitesInstaller
    {
        readonly IDialogManager _dialogManager;
        readonly IRepoActionHandler _repoActionHandler;
        readonly IToolsInstaller _toolsInstaller;

        public PreRequisitesInstaller(IDialogManager dialogManager, IRepoActionHandler repoActionHandler,
            IToolsInstaller toolsInstaller) {
            _dialogManager = dialogManager;
            _repoActionHandler = repoActionHandler;
            _toolsInstaller = toolsInstaller;
        }

        public async Task InstallPreRequisites() {
            await InstallToolsIfNeeded().ConfigureAwait(false);
        }

        async Task InstallToolsIfNeeded() {
            try {
                await TryInstallToolsIfNeeded().ConfigureAwait(false);
            } catch (Exception e) {
                _dialogManager.ExceptionDialogSync(e,
                    "Tools failed to download, Play withSIX will not function correctly without these files. " +
                    "Verify that your Internet connection is working and restart to try again.",
                    "A problem occurred during tools download");
                await UserError.Throw(new InformationalUserError(e, "A problem occurred during tools download", null));
            }
        }

        async Task TryInstallToolsIfNeeded() {
            if (await _toolsInstaller.ConfirmToolsInstalled(true).ConfigureAwait(false))
                return;

            using (var repo = new StatusRepo {Action = RepoStatus.Downloading}) {
                await _repoActionHandler.PerformStatusActionWithBusyHandlingAsync(repo, "Tools",
                    () => _toolsInstaller.DownloadAndInstallTools(repo)).ConfigureAwait(false);
            }
        }
    }
}