// <copyright company="SIX Networks GmbH" file="PlayShellViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Applications.NotificationHandlers;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Profiles;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Applications.ViewModels.Popups;
using SN.withSIX.Play.Applications.Views.Dialogs;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Sync.Core.Transfer;
using IScreen = Caliburn.Micro.IScreen;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels
{
    [DoNotObfuscate]
    public class PlayShellViewModel : WindowBase, IPlayShellViewModel,
        IHandle<NewVersionDownloaded>,
        IHandle<GameContentInitialSynced>,
        IHandle<GameLaunchedEvent>,
        IHandle<ProcessAppEvent>,
        IHandle<RequestOpenLogin>
    {
        const double DefaultContactListWidth = 200;
        readonly Subject<bool> _activateWindows = new Subject<bool>();
        readonly Lazy<ContentViewModel> _contentLazy;
        readonly Lazy<IContentManager> _contentManager;
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly LocalMachineInfo _machineInfo;
        readonly IMediator _mediator;
        readonly IProcessManager _processManager;
        readonly IRestarter _restarter;
        readonly IShutdownHandler _shutdownHandler;
        readonly IPlayStartupManager _startupManager;
        readonly ISystemInfo _systemInfo;
        bool _canGoBack;
        bool _canGoForward;
        ConnectViewModel _connect;
        string _connectionStatus;
        double _contactListWidth;
        Server _favoriteServer;
        bool _goingBack;
        bool _goingForward;
        bool? _gridMode;
        IHaveOverlay _hasOverlay;
        bool _isApplicationShown = true;
        bool _loaded;
        double _maxWidth = double.PositiveInfinity;
        double _minWidth = double.NaN;
        int _screenIndex;
        string _securityStatus;
        string _status;
        OverlayViewModelBase _subOverlay;
        bool _wasMaximized;
        WindowState _windowState = WindowState.Normal;

        public PlayShellViewModel(IPlayStartupManager startupManager, IDialogManager dialogManager,
            IProcessManager processManager,
            IEventAggregator eventBus, ISystemInfo systemInfo,
            Func<NotificationsCenterViewModel> notificationsCenter, IStatusViewModel statusViewModel,
            SettingsViewModel settings, HomeViewModel home,
            Lazy<IContentManager> contentManager,
            Lazy<ContentViewModel> contentLazy,
            IUpdateManager updateManager, IViewModelFactory factory,
            ISoftwareUpdate softwareUpdate, ConnectViewModel connect, LocalMachineInfo machineInfo,
            UserSettings userSettings, IShutdownHandler shutdownHandler, IMediator mediator, IRestarter restarter) {
            _contentLazy = contentLazy;
            _restarter = restarter;
            using (this.Bench()) {
                _startupManager = startupManager;
                _dialogManager = dialogManager;
                _processManager = processManager;
                _eventBus = eventBus;
                _systemInfo = systemInfo;
                _contentManager = contentManager;
                UpdateManager = updateManager;
                SoftwareUpdate = softwareUpdate;
                UserSettings = userSettings;
                _shutdownHandler = shutdownHandler;
                _mediator = mediator;
                _machineInfo = machineInfo;

                Router = new RoutingState();

                Factory = factory;
                Connect = connect;
                Home = home;
                Settings = settings;
                StatusFlyout = statusViewModel;

                NotificationsCenter = notificationsCenter();
                // TODO: Normally we would do this only on a user action, like when we would open the menu. It would require the Shell button to be separate from the menu though.
                ProfilesMenu = _mediator.Request(new GetProfilesMenuViewModelQuery());

                DisplayName = GetTitle();

                DidDetectAVRun = _systemInfo.DidDetectAVRun;
                SecurityStatus = _startupManager.GetSecurityWarning();
                TrayIconContextMenu = new TrayIconContextMenu(this);
                RecentJumpList = new RecentJumpList();
                OptionsMenu = new OptionsMenuViewModel(this);
                ScreenHistory = new ReactiveList<object>();

                Activator = new ViewModelActivator();

                Application.Current.Exit += (sender, args) => UpdateManager.Terminate();

                Overlay = new OverlayConductor();

                this.SetCommand(x => x.TrayIconDoubleclicked).Subscribe(x => SwitchWindowState());

                this.SetCommand(x => x.Exit).Subscribe(x => {
                    if (!IsBusy())
                        _shutdownHandler.Shutdown();
                });

                this.SetCommand(x => x.GoPremiumCommand)
                    .Subscribe(x => BrowserHelper.TryOpenUrlIntegrated(new Uri(CommonUrls.MainUrl, "/gopremium")));
                this.SetCommand(x => x.GoPremiumSettingsCommand)
                    .Subscribe(
                        x => BrowserHelper.TryOpenUrlIntegrated(new Uri(CommonUrls.ConnectUrl, "/me/settings/premium")));
                this.SetCommand(x => x.SwitchHome).Subscribe(x => SwitchHomeButton());
                this.SetCommand(x => x.GoLatestNewsCommand).Subscribe(x => GoLatestNews());
                this.SetCommand(x => x.SecuritySuiteCommand).Subscribe(x => BrowserHelper.TryOpenUrlIntegrated(
                    "https://community.withsix.com"));

                this.SetCommand(x => x.GoBackCommand, this.WhenAnyValue(x => x.CanGoBack))
                    .Subscribe(GoBack);
                this.SetCommand(x => x.GoForwardCommand, this.WhenAnyValue(x => x.CanGoForward))
                    .Subscribe(GoForward);
            }
        }

        protected PlayShellViewModel() {}
        public ReactiveList<object> ScreenHistory { get; }
        public ReactiveCommand GoLatestNewsCommand { get; private set; }
        public RecentJumpList RecentJumpList { get; private set; }
        public ReactiveCommand GoPremiumCommand { get; protected set; }
        public ReactiveCommand GoPremiumSettingsCommand { get; protected set; }
        public ReactiveCommand GoBackCommand { get; protected set; }
        public ReactiveCommand GoForwardCommand { get; protected set; }
        public ReactiveCommand SwitchHome { get; protected set; }
        public IUpdateManager UpdateManager { get; }
        public Server FavoriteServer
        {
            get { return _favoriteServer; }
            set { SetProperty(ref _favoriteServer, value); }
        }
        public bool LockDown
        {
            get { return Common.Flags.LockDown; }
        }
        public bool DidDetectAVRun { get; set; }
        public ReactiveCommand SecuritySuiteCommand { get; private set; }
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        public string SecurityStatus
        {
            get { return _securityStatus; }
            set { SetProperty(ref _securityStatus, value); }
        }
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set { SetProperty(ref _connectionStatus, value); }
        }
        public HomeViewModel Home { get; protected set; }
        public ConnectViewModel Connect
        {
            get { return _connect; }
            protected set { SetProperty(ref _connect, value); }
        }
        public NotificationsCenterViewModel NotificationsCenter { get; private set; }
        public ProfilesMenuViewModel ProfilesMenu { get; private set; }
        public OptionsMenuViewModel OptionsMenu { get; private set; }
        public double MaxWidth
        {
            get { return _maxWidth; }
            set { SetProperty(ref _maxWidth, value); }
        }
        public double MinWidth
        {
            get { return _minWidth; }
            set { SetProperty(ref _minWidth, value); }
        }
        public override WindowState WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }
        public double ContactListWidth
        {
            get { return _contactListWidth; }
            protected set { SetProperty(ref _contactListWidth, value); }
        }
        public UserSettings UserSettings { get; }
        public int ScreenIndex
        {
            get { return _screenIndex; }
            set { SetProperty(ref _screenIndex, value); }
        }
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { SetProperty(ref _canGoBack, value); }
        }
        public bool CanGoForward
        {
            get { return _canGoForward; }
            set { SetProperty(ref _canGoForward, value); }
        }
        IHaveOverlay HasOverlay
        {
            get { return _hasOverlay; }
            set { SetProperty(ref _hasOverlay, value); }
        }

        public void Handle(RequestOpenLogin message) {
            Common.App.Events.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.AccountSettingsUrl));
        }

        public ReactiveCommand Exit { get; protected set; }
        public ISoftwareUpdate SoftwareUpdate { get; }
        public ViewModelActivator Activator { get; }
        public IStatusViewModel StatusFlyout { get; }
        public IViewModelFactory Factory { get; }
        public bool? GridMode
        {
            get { return _gridMode; }
            set { SetProperty(ref _gridMode, value); }
        }
        public IContentViewModel Content
        {
            get { return _contentLazy.Value; }
        }
        public SettingsViewModel Settings { get; }
        public OverlayConductor Overlay { get; }
        public ContextMenuBase TrayIconContextMenu { get; }
        public ReactiveCommand TrayIconDoubleclicked { get; private set; }

        public void ShowOverlay(OverlayViewModelBase overlay) {
            Overlay.ActivateItem(overlay);
        }

        public void CloseOverlay() {
            var activeItem = Overlay.ActiveItem;
            if (activeItem != null)
                Overlay.CloseItem(activeItem);
        }

        public override void CanClose(Action<bool> callback) {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Common.Flags.ShuttingDown)
                NormalShutdown(callback);
            else if (IsOverlayEnabled()) {
                if (UserSettings.AppOptions.CloseCurrentOverlay)
                    CloseOverlays(callback);
                else if (UserSettings.AppOptions.RememberWarnOnOverlayOpen) {
                    if (UserSettings.AppOptions.WarnOnOverlayOpen)
                        ShutdownOrMinimize(callback);
                } else
                    ShowOverlayWarning(callback);
            } else
                ShutdownOrMinimize(callback);
        }

        public RoutingState Router { get; }
        public OverlayViewModelBase SubOverlay
        {
            get { return _subOverlay; }
            set { SetProperty(ref _subOverlay, value); }
        }
        public IObservable<bool> ActivateWindows
        {
            get { return _activateWindows.AsObservable(); }
        }

        void GoBack(object x) {
            if (Content.ActiveItem == Home && Home.CanGoBack) {
                Home.GoBack();
                return;
            }
            ScreenIndex -= 1;
            _goingBack = true;
            Content.ActivateItem(ScreenHistory[ScreenIndex]);
        }

        void GoForward(object x) {
            if (Content.ActiveItem == Home && Home.CanGoForward) {
                Home.GoForward();
                return;
            }
            ScreenIndex += 1;
            _goingForward = true;
            Content.ActivateItem(ScreenHistory[ScreenIndex]);
        }

        bool IsOverlayEnabled() {
            return SubOverlay != null || Overlay.ActiveItem != null;
        }

        [DoNotObfuscate, SmartAssembly.Attributes.ReportUsage]
        public void ToggleUpdater() {
            StatusFlyout.IsOpen = !StatusFlyout.IsOpen;
        }

        [DoNotObfuscate]
        public void GoLatestNews() {
            _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(Home.GetLatestNewsLink()));
        }

        void SwitchHomeButton() {
            if (!Home.IsActive)
                Content.ActivateItem(Home);
            else
                Home.GoHome();
        }

        void ShutdownOrMinimize(Action<bool> callback) {
            if (UserSettings.AppOptions.CloseMeansMinimize)
                MinimizeToTray(callback);
            else
                NormalShutdown(callback);
        }

        void ShowOverlayWarning(Action<bool> callback) {
            var r =
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams("An overlay is open, do you still want to close the application?",
                        "Overlay open. Are you sure you wish to close?", SixMessageBoxButton.YesNo) {
                            RememberedState = false
                        });

            if (r == SixMessageBoxResult.YesRemember) {
                Settings.RememberWarnOnOverlayOpen = true;
                Settings.WarnOnOverlayOpen = true;
            } else if (r == SixMessageBoxResult.NoRemember) {
                Settings.RememberWarnOnOverlayOpen = true;
                Settings.WarnOnOverlayOpen = false;
            }

            if (r.IsYes())
                ShutdownOrMinimize(callback);
        }

        void MinimizeToTray(Action<bool> callback) {
            if (UserSettings.AppOptions.FirstTimeMinimizedToTray) {
                _mediator.Notify(new MinimizedEvent());
                UserSettings.AppOptions.FirstTimeMinimizedToTray = false;
            }
            Hide();
            callback(false);
        }

        void CloseOverlays(Action<bool> callback) {
            var o = Overlay.ActiveItem;
            if (o != null)
                o.TryClose();
            else {
                var p = SubOverlay;
                if (p != null)
                    p.TryClose();
            }

            callback(false);
        }

        void NormalShutdown(Action<bool> callback) {
            if (IsBusy()) {
                callback(false);
                return;
            }

            base.CanClose(callback);
        }

        void SwitchWindowState() {
            if (_isApplicationShown)
                Hide();
            else
                Show();
        }

        void RegisterAppEvents() {
            var app = ((ISingleInstanceApp) Application.Current);
            app.Activated += (sender, args) => {
                if (!Common.AppCommon.IsBusy && !Debugger.IsAttached)
                    _machineInfo.Update();
            };

            var le = app.LastAppEvent;
            if (le != null)
                HandleAppEvent(null, le);
            app.AppEvent += HandleAppEvent;
        }

        bool IsBusy() {
            if (!Common.AppCommon.IsBusy)
                return false;

            if (UserSettings.AppOptions.RememberWarnOnBusyShutdown)
                return !UserSettings.AppOptions.WarnOnBusyShutdown;

            var r =
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "It appears mods are being installed/updated currently, are you sure you wish to quit?",
                        "Processes running. Are you sure you wish to exit?", SixMessageBoxButton.YesNo) {
                            RememberedState = false
                        });

            if (r == SixMessageBoxResult.YesRemember) {
                Settings.RememberWarnOnBusyShutdown = true;
                Settings.WarnOnBusyShutdown = true;
            } else if (r == SixMessageBoxResult.NoRemember) {
                Settings.RememberWarnOnBusyShutdown = true;
                Settings.WarnOnBusyShutdown = false;
            }

            return !r.IsYes();
        }

        string GetTitle(string customTitle = null) {
            string title;
            if (Common.Flags.LockDown) {
                if (customTitle == null) {
                    var ms = Common.Flags.LockDownModSet;
                    customTitle = ms == null ? null : ms.Replace("_", " ").UppercaseFirst();
                }
                const int max = 20;
                if (customTitle != null && customTitle.Length > max)
                    customTitle = customTitle.Substring(0, max - 1) + "..";
                var state = string.IsNullOrWhiteSpace(Common.AppCommon.ApplicationState)
                    ? String.Empty
                    : " " + Common.AppCommon.ApplicationState;
                title = String.Format("Play {0} withSIX{1}", customTitle, state);
            } else
                title = ApplicationTitle();

            return title + Common.AppCommon.TitleType;
        }

        string ApplicationTitle() {
            return String.Join(";",
                new[] {Common.AppCommon.ApplicationName, Common.AppCommon.ApplicationState}.Where(
                    x => !String.IsNullOrWhiteSpace(x)));
        }

        protected override async void OnViewLoaded(object view) {
            using (this.Bench()) {
                base.OnViewLoaded(view);

                // WARNING - DO NOT SHOW NON-FATAL DIALOGS BEFORE THIS POINT!
                if (_loaded)
                    return;
                _loaded = true;

                if (!_systemInfo.IsInternetAvailable)
                    Connect.IsEnabled = false;
                await Task.Run(() => _startupManager.VisualInit()).ConfigureAwait(false);
            }
        }

        void InitialWindowHandling() {
            if (UserSettings.WindowSettings != null)
                UserSettings.WindowSettings.Apply(this);
            else
                UserSettings.WindowSettings = WindowSettings.Create(this);

            if (UserSettings.AppOptions.AutoAdjustLibraryViewType)
                UpdateGridMode();
        }

        async void HandleAppEvent(object obj, IList<string> data) {
            _activateWindows.OnNext(true);
            await TryProcessParams(data.Skip(1).ToArray());
        }

        Task TryProcessParams(IEnumerable<string> @params) {
            //Contract.Requires<ArgumentNullException>(@params != null);
            return UiTaskHandler.TryAction(() => ProcessParams(@params),
                "Processing of startup parameters");
        }

        async Task ProcessParams(IEnumerable<string> @params) {
            foreach (var p in @params) {
                this.Logger().Info("Processing Param: {0}", p);

                if (p.StartsWith("--profile=")) {
                    var desiredProfile = p.Replace("--profile=", "");
                    _mediator.Request(new SwitchProfileByNameCommand(desiredProfile));
                    await Task.Delay(3.Seconds()).ConfigureAwait(false);
                }

                if (p.Contains("lockdown=")) {
                    if (Common.AppCommon.IsBusy) {
                        UsageCounter.ReportUsage("Dialog - Cannot restart in lockdown mode");
                        await _dialogManager.MessageBoxAsync(
                            new MessageBoxDialogParams("Cannot restart in lockdown mode, currently busy!\n" + p));
                        return;
                    }
                    _restarter.RestartWithoutElevation(p);
                    return;
                }

                if (IsPwsUrl(p)) {
                    retry:
                    Exception e = null;
                    try {
                        await HanldePwsUrl(p).ConfigureAwait(false);
                    } catch (DownloadException ex) {
                        e = ex;
                    }
                    if (e != null) {
                        var result = await UserError.Throw(AddRepositoryViewModel.HandleException(e));
                        if (result == RecoveryOptionResult.RetryOperation)
                            goto retry;
                        // TODO: Strategy
                        throw e;
                    }
                }
                if (IsWebUrl(p))
                    _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(p));
            }
        }

        async Task HanldePwsUrl(string p) {
            try {
                await _contentManager.Value.HandlePwsUrl(p).ConfigureAwait(false);
            } catch (BusyStateHandler.BusyException) {
                _dialogManager.BusyDialog();
            }
        }

        static bool IsPwsUrl(string p) {
            return SixRepo.URLSchemes.Select(x => x + "://")
                .Any(x => p.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        static bool IsWebUrl(string p) {
            return new[] {"http://", "https://"}
                .Any(x => p.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        [DoNotObfuscate]
        public void LaunchApp(ExternalApp app) {
            app.Launch(_processManager);
        }

        void DealWithState(bool show) {
            _isApplicationShown = show;
            _activateWindows.OnNext(show);
        }

        void Hide() {
            StoreMaximizedState();
            SetToNormalIfMinimized();
            DealWithState(false);
        }

        void Show() {
            SetMaximizedIfWasMaximized();
            DealWithState(true);
        }

        void StoreMaximizedState() {
            _wasMaximized = WindowState == WindowState.Maximized;
        }

        void SetMaximizedIfWasMaximized() {
            if (_wasMaximized)
                WindowState = WindowState.Maximized;
        }

        void SetToNormalIfMinimized() {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
        }

        void UpdateGridMode() {
            var width = Connect.IsEnabled ? Width - ContactListWidth : Width;
            GridMode = DetermineGridMode(width);
        }

        bool DetermineGridMode(double width) {
            return UserSettings.AppOptions.AutoAdjustLibraryViewType &&
                   (width >= 1280 || WindowState == WindowState.Maximized);
        }

        protected override void OnInitialize() {
            using (this.Bench()) {
                base.OnInitialize();

                this.WhenAnyValue(x => x.Connect.IsEnabled)
                    .Select(x => x ? DefaultContactListWidth : 0).BindTo(this, x => x.ContactListWidth);

                // Workaround for screens..
                object previous = null;
                this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                    .Subscribe(x => {
                        var deactivatable = previous as IDeactivate;
                        if (deactivatable != null)
                            deactivatable.Deactivate(false);
                        var activable = x as IScreen;
                        if (activable != null)
                            activable.Activate();
                        previous = x;
                    });

                this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                    .Select(x => x as IHaveOverlay)
                    .BindTo(this, x => x.HasOverlay);

                this.WhenAnyValue(x => x.HasOverlay.Overlay)
                    .BindTo(this, x => x.SubOverlay);

                var dm = Common.Flags.Verbose ? " (diagnostics mode)" : null;
                _systemInfo.WhenAnyValue(x => x.IsInternetAvailable)
                    .Select(x => (x ? "Ready" : "Offline") + dm)
                    .BindTo(this, x => x.ConnectionStatus);

                this.WhenAnyValue(x => x.UserSettings.AppOptions.AutoAdjustLibraryViewType, x => x.Width,
                    x => x.WindowState,
                    x => x.Connect.IsEnabled,
                    (w, x, y, z) => w)
                    .Where(x => x)
                    .Subscribe(x => UpdateGridMode());

                InitialWindowHandling();

                this.WhenAnyValue(x => x.Content.ActiveItem)
                    .Subscribe(ActiveItemChanged);

                this.WhenAnyValue(x => x.Home.CanGoBack, x => x.Home.CanGoForward, x => x.ScreenHistory.Count,
                    x => x.ScreenIndex, x => x.Content.ActiveItem,
                    (b, f, c, i, m) =>
                        new {
                            CanGoBack = (c > 1 && i > 0) || (m == Home && Home.CanGoBack),
                            CanGoForward =
                                (c > 1 && i < ScreenHistory.Count() - 1) ||
                                (m == Home && Home.CanGoForward)
                        }).Subscribe(x => {
                            CanGoBack = x.CanGoBack;
                            CanGoForward = x.CanGoForward;
                        });

                Overlay.ConductWith(this);
                Overlay.Parent = this;
                Connect.ConductWith(this);
                Connect.Parent = this;

                Router.Navigate.Execute(Content);
            }
        }

        void ActiveItemChanged(object x) {
            lock (ScreenHistory) {
                //var goingBack = ScreenIndex < ScreenHistory.Count() - 1;
                if (_goingBack || _goingForward) {
                    _goingBack = false;
                    _goingForward = false;
                } else {
                    var newIndex = ScreenIndex + 1;
                    var toRemove = ScreenHistory.Count() - newIndex;
                    if (toRemove > 0)
                        ScreenHistory.RemoveRange(newIndex, toRemove);
                    ScreenHistory.Add(x);
                    ScreenIndex = newIndex;
                }
            }
        }

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);
            UserSettings.WindowSettings = WindowSettings.Create(this);
        }

        #region IHandle events

        public async void Handle(GameContentInitialSynced message) {
            await TryProcessParams(Common.Flags.StartupParameters).ConfigureAwait(false);
            RegisterAppEvents();
        }

        public void Handle(NewVersionDownloaded message) {
            SoftwareUpdate.UpdateAndExitIfNotBusy();
        }

        public void Handle(GameLaunchedEvent message) {
            if (UserSettings.GameOptions.CloseOnLaunch) {
                Thread.Sleep(1000);
                _shutdownHandler.Shutdown();
            }

            if (UserSettings.GameOptions.MinimizeOnLaunch)
                WindowState = WindowState.Minimized;
        }

        public void Handle(ProcessAppEvent message) {
            TryProcessParams(new[] {message.URL});
        }

        #endregion
    }
}