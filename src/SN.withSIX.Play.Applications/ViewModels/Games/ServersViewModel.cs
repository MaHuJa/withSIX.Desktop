// <copyright company="SIX Networks GmbH" file="ServersViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Options;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    [DoNotObfuscate]
    public class ServersViewModel : LibraryModuleViewModel,
        IHandle<ServersAdded>, IHandle<ServersUpdated>,
        IHandle<GameContentAfterSynced>, IHandle<GameLaunchedEvent>,
        IHandle<CalculatedGameSettingsUpdated>, IHandle<ActiveGameChangedForReal>, IHandle<GameContentInitialSynced>
    {
        public static readonly SortData[] RequiredColumns = {
            new SortData {
                DisplayName = "IsFavorite",
                Value = "IsFavorite",
                SortDirection = ListSortDirection.Descending
            },
            new SortData {
                DisplayName = "IsFeatured",
                Value = "IsFeatured",
                SortDirection = ListSortDirection.Descending
            },
            new SortData {
                DisplayName = "HasFriends",
                Value = "HasFriends",
                SortDirection = ListSortDirection.Descending
            }
        };
        public static readonly SortData[] Columns = {
            new SortData {DisplayName = "Name", Value = "Name"},
            new SortData {
                DisplayName = "NumPlayers",
                Value = "NumPlayers",
                SortDirection = ListSortDirection.Descending
            },
            new SortData {
                DisplayName = "MaxPlayers",
                Value = "MaxPlayers"
            },
            new SortData {
                DisplayName = "Difficulty",
                Value = "Difficulty"
            },
            new SortData {DisplayName = "Ping", Value = "Ping"}
            ,
            new SortData {
                DisplayName = "Mission",
                Value = "Mission"
            },
            new SortData {
                DisplayName = "Map",
                Value = "Island"
            },
            new SortData {
                DisplayName = "GameVer",
                Value = "GameVer"
            },
            new SortData {
                DisplayName = "GameMode",
                Value = "GameMode"
            },
            new SortData {
                DisplayName = "GameType",
                Value = "GameType"
            },
            new SortData {
                DisplayName = "LastJoinedOn",
                Value = "LastJoinedOn"
            },
            new SortData {
                DisplayName = "ServerTime",
                Value = "ServerTime"
            }
        };
        readonly IContentManager _contentManager;
        readonly IDialogManager _dialogManager;
        readonly IViewModelFactory _factory;
        readonly LaunchManager _launchManager;
        readonly UserSettings _settings;
        string _addServerInput;
        int _addServerPort = 2302;
        IDisposable _filterSubscription;
        bool _isAddServerVisible;
        Collection _lastContent;
        Game _lastGame;
        ServerLibraryViewModel _libraryVm;
        int? _maxPlayers;
        int? _numPlayers;
        bool _quickPlayEnabled;
        IFilter _serverFilter;
        IServerList _serverList;
        int? _serversCount;
        protected ServersViewModel() {}

        public ServersViewModel(ServerInfoOverlayViewModel siovm, IDialogManager dialogManager, UserSettings settings,
            SettingsViewModel settingsVM, LaunchManager launchManager, IContentManager contentManager,
            ISystemInfo systemInfo, IViewModelFactory factory) {
            _settings = settings;
            _launchManager = launchManager;
            _contentManager = contentManager;
            SystemInfo = systemInfo;
            _dialogManager = dialogManager;
            Settings = settingsVM;
            ServerInfoOverlay = siovm;
            _factory = factory;

            DisplayName = "Servers";
            ModuleName = ControllerModules.ServerBrowser;
            AssignServerList();
        }

        public ServerLibraryViewModel LibraryVM
        {
            get { return _libraryVm; }
            protected set { SetProperty(ref _libraryVm, value); }
        }
        public ISystemInfo SystemInfo { get; }
        public bool LockDown
        {
            get { return Common.Flags.LockDown; }
        }
        public ServerInfoOverlayViewModel ServerInfoOverlay { get; protected set; }
        public ReactiveCommand QActionCommand { get; private set; }
        public ReactiveCommand AddServerCommand { get; private set; }
        public ReactiveCommand AddServerOKCommand { get; private set; }
        public ReactiveCommand AddServerCancelCommand { get; private set; }
        public bool QuickPlayEnabled
        {
            get { return _quickPlayEnabled; }
            set { SetProperty(ref _quickPlayEnabled, value); }
        }
        public IFilter ServerFilter
        {
            get { return _serverFilter; }
            protected set { SetProperty(ref _serverFilter, value); }
        }
        public SettingsViewModel Settings { get; protected set; }
        public IServerList ServerList
        {
            get { return _serverList; }
            set { SetProperty(ref _serverList, value); }
        }
        public ReactiveCommand<Unit> ReloadCommand { get; private set; }
        public int? NumPlayers
        {
            get { return _numPlayers; }
            set { SetProperty(ref _numPlayers, value); }
        }
        public int? ServersCount
        {
            get { return _serversCount; }
            set { SetProperty(ref _serversCount, value); }
        }
        public int? MaxPlayers
        {
            get { return _maxPlayers; }
            set { SetProperty(ref _maxPlayers, value); }
        }
        public bool IsAddServerVisible
        {
            get { return _isAddServerVisible; }
            set { SetProperty(ref _isAddServerVisible, value); }
        }
        public string AddServerInput
        {
            get { return _addServerInput; }
            set
            {
                if (_addServerInput == value)
                    return;
                //OnPropertyChanging();
                if (value != null) {
                    var str = value;
                    _addServerInput = str.Trim();
                } else
                    _addServerInput = value;
                OnPropertyChanged();
            }
        }
        public int AddServerPort
        {
            get { return _addServerPort; }
            set { SetProperty(ref _addServerPort, value); }
        }
        public ReactiveCommand SwitchQuickPlay { get; private set; }
        public ReactiveCommand<Unit> AbortCommand { get; private set; }
        public ReactiveCommand<Unit> RefreshFilterCommand { get; private set; }
        public ReactiveCommand<Unit> CgsUpdate { get; private set; }

        public void Handle(ActiveGameChangedForReal message) {
            if (!message.Game.SupportsServers())
                return;
            AssignServerList();
            LibraryVM.Setup();
        }

        public void Handle(GameContentInitialSynced message) {
            if (LibraryVM != null)
                LibraryVM.Setup();
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            var sl = this.WhenAnyValue(x => x.ServerList.IsUpdating, x => x.SystemInfo.IsInternetAvailable,
                (updating, internetAvailable) => !updating && internetAvailable)
                .ObserveOn(RxApp.MainThreadScheduler);
            var processingObservable = this.WhenAnyValue(x => x.ServerList.DownloadingServerList,
                x => x.ServerList.IsUpdating)
                .ObserveOn(RxApp.MainThreadScheduler);
            ReactiveUI.ReactiveCommand.CreateAsyncTask(processingObservable.Select(x => !x.Item1 && !x.Item2),
                async x => await Reload())
                .SetNewCommand(this, x => x.ReloadCommand)
                .Subscribe();
            ReactiveUI.ReactiveCommand.CreateAsyncTask(processingObservable.Select(x => x.Item1 || x.Item2),
                async x => await AbortInternal())
                .SetNewCommand(this, x => x.AbortCommand)
                .Subscribe();
            this.SetCommand(x => x.SwitchQuickPlay).Subscribe(SwitchQuickPlayAction);
            this.SetCommand(x => x.QActionCommand).RegisterAsyncTask(QAction).Subscribe();
            this.SetCommand(x => x.AddServerCommand).Subscribe(x => ShowAddServer());
            this.SetCommand(x => x.AddServerOKCommand).RegisterAsyncTask(AddServerOK).Subscribe();
            this.SetCommand(x => x.AddServerCancelCommand).Subscribe(x => CancelAddServer());

            RefreshFilterCommand = ReactiveUI.ReactiveCommand.CreateAsyncTask(x => RefreshFilterWithDelay());

            CreateLibrary(DomainEvilGlobal.SelectedGame.ActiveGame);

            _contentManager.InitialServerSync(true);

            CgsUpdate = ReactiveUI.ReactiveCommand.CreateAsyncTask(OnCgsUpdate).DefaultSetup("OnCgsUpdate");
            CgsUpdate.Subscribe();

            var onActivate =
                ReactiveUI.ReactiveCommand.CreateAsyncTask(x => OnActivateInternal()).DefaultSetup("OnActivate");
            onActivate.Subscribe();
            var isEnabledQuery = this.WhenAnyValue(x => x.IsActive);
            isEnabledQuery.Subscribe(x => _settings.AppOptions.ServerListEnabled = x);
            isEnabledQuery.Where(x => x)
                .Skip(1)
                .Subscribe(x => onActivate.Execute(null));

            this.WhenAnyValue(x => x.IsActive)
                .Skip(1)
                .Subscribe(value => UsageCounter.ReportUsage("Tab Servers"));

            this.WhenAnyValue(x => x.ServerList.ServerQueryQueue.State)
                .Subscribe(x => ProgressState = x);

            this.WhenAnyValue(x => x.ServerList.DownloadingServerList)
                .Subscribe(SwitchProgressState);
        }

        async Task RefreshFilterWithDelay() {
            // This little RefreshFilter bugger here will make switching to the ServerList slow.. so let's schedule it slightly later.. 
            // TODO: Evaluate if using different UI thread priorities could help..
            await Task.Delay(500).ConfigureAwait(false);
            await RefreshFilter().ConfigureAwait(false);
        }

        async Task OnActivateInternal() {
            await RefreshFilterCommand.ExecuteAsyncTask().ConfigureAwait(false);

            var ag = DomainEvilGlobal.SelectedGame.ActiveGame;
            if (ag.CalculatedSettings.InitialSynced && !ServerList.InitialSync) {
                await _contentManager.InitialServerSync().ConfigureAwait(false);
                return;
            }

            var am = ag == null ? null : ag.CalculatedSettings.Collection;
            if (_lastContent == am && _lastGame == ag)
                return;
            _lastGame = ag;
            _lastContent = am;
            if (ServerList.InitialSync)
                await ServerList.UpdateServers().ConfigureAwait(false);
        }

        void SwitchProgressState(bool isDownloading) {
            if (isDownloading)
                ProgressState = new ProgressState {IsIndeterminate = true, Active = true};
            else
                ProgressState = ServerList.ServerQueryQueue.State;
        }

        void CreateLibrary(Game game) {
            if (game.SupportsServers())
                UiHelper.TryOnUiThread(() => LibraryVM = _factory.CreateServerLibraryViewModel(game, this).Value);
            else
                LibraryVM = null;
        }

        Task Reload() {
            return ServerList.InitialSync ? ServerList.UpdateServers() : _contentManager.InitialServerSync();
        }

        void ResetCounts() {
            NumPlayers = null;
            MaxPlayers = null;
            ServersCount = null;
        }

        void AssignServerList() {
            ServerList = _contentManager.ServerList;
        }

        void HandleServerFilters(Game newGame) {
            if (_filterSubscription != null)
                _filterSubscription.Dispose();
            if (newGame.SupportsServers())
                _filterSubscription = SetupServerFilters(newGame.Servers());
        }

        IDisposable SetupServerFilters(ISupportServers game) {
            ServerFilter = game.GetServerFilter();
            return ServerFilter.FilterChanged.Subscribe(x => RefreshFilterCommand.Execute(null));
        }

        [DoNotObfuscate, SmartAssembly.Attributes.ReportUsage]
        public void SelectNoServer() {
            LibraryVM.ActiveItem = null;
        }

        [SmartAssembly.Attributes.ReportUsage]
        void SwitchQuickPlayAction(object x) {
            QuickPlayEnabled = !QuickPlayEnabled;
        }

        async Task RefreshFilter() {
            var items = LibraryVM.ItemsView;
            if (items == null)
                return;

            // TODO This is a place of problem "Destination array was not long enough"
            // It happened without specific user interaction, so probably not related to filter etc.
            // Because edits coming in from ServerList servers.SyncCollectionLocked(Items);
            // ---
            // Currently worked around by catching the exception by using TryRefresh..
            foreach (var item in items.SourceCollection.OfType<ServerLibraryItemViewModel>())
                await Execute.OnUIThreadAsync(() => item.ItemsView.TryRefreshIfHasView());
            await Execute.OnUIThreadAsync(ProcessModServers);
        }

        void ProcessModServers() {
            if (ServerList.DownloadingServerList || !ServerList.InitialSync) {
                ResetCounts();
                return;
            }

            var list = GetFilteredServers();

            if (list == null)
                return;
            NumPlayers = list.Sum(x => x.NumPlayers);
            MaxPlayers = list.Sum(x => x.MaxPlayers);
            ServersCount = list.Length;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ServerInfo(Server server) {
            ShowOverlay(ServerInfoOverlay);
        }

        [SmartAssembly.Attributes.ReportUsage, DoNotObfuscate]
        public async void Abort() {
            await AbortInternal().ConfigureAwait(false);
        }

        Task AbortInternal() {
            return Task.Run(() => ServerList.AbortSync());
        }

        [SmartAssembly.Attributes.ReportUsage]
        async Task QAction() {
            var servers = _settings.ServerOptions.QuickPlayApplyServerFilters
                ? GetFilteredServers()
                : ServerList.Items.ToArray();

            if (!servers.Any()) {
                await
                    _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("No servers found, adjust your filters?"));
                return;
            }

            var theServer = servers
                .Where(x => !x.PasswordRequired)
                .FirstOrDefault(
                    server =>
                        server.NumPlayers > _settings.ServerOptions.MinNumPlayers &&
                        server.FreeSlots > _settings.ServerOptions.MinFreeSlots);

            if (theServer == null) {
                await
                    _dialogManager.MessageBoxAsync(
                        new MessageBoxDialogParams("No matching server found, adjust your filters?"));
                return;
            }

            LibraryVM.ActiveItem = theServer;
            await _launchManager.JoinServer().ConfigureAwait(false);
        }

        // TODO: Should become available per view or so
        Server[] GetFilteredServers() {
            var serverLibraryItem = LibraryVM.SelectedItem.FindItem<ServerLibraryItemViewModel>();
            if (serverLibraryItem == null)
                return new Server[0];
            var vs = serverLibraryItem.ItemsView;
            return vs == null ? new Server[0] : vs.ToArray<Server>();
        }

        [SmartAssembly.Attributes.ReportUsage]
        void ShowAddServer() {
            IsAddServerVisible = !IsAddServerVisible;
        }

        [SmartAssembly.Attributes.ReportUsage]
        async Task AddServerOK() {
            var str = AddServerInput;

            if (String.IsNullOrWhiteSpace(str)) {
                UsageCounter.ReportUsage("Dialog - Specify a valid address");
                await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        "Please specify a valid ip address or hostname, specify the port separately down below"));
                return;
            }

            IPAddress ip;
            try {
                ip = SAStuff.GetValidIp(str);
            } catch (SocketException e) {
                UsageCounter.ReportUsage("Dialog - Specify a valid address");
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "Please specify a valid ip address or hostname, specify the port separately down below\nError: " +
                        e.Message));
                return;
            }

            var port = AddServerPort;
            if (port < 1 || port > IPEndPoint.MaxPort) {
                UsageCounter.ReportUsage("Dialog - Specify a valid port");
                await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("Please specify a valid port"));
                return;
            }

            await Task.Run(() => {
                var server = TryFindOrCreateServer(new ServerAddress(ip, port));
                if (server == null)
                    return;

                server.IsFavorite = true;
                server.TryUpdateAsync().ConfigureAwait(false);
                LibraryVM.ActiveItem = server;
                AddServerInput = null;
                IsAddServerVisible = false;
            });
        }

        Server TryFindOrCreateServer(ServerAddress str) {
            try {
                return ServerList.FindOrCreateServer(str);
            } catch (SocketException e) {
                _dialogManager.ExceptionDialogSync(e, "Failed to add server, is the address valid?");
            } catch (ArgumentException e) {
                _dialogManager.ExceptionDialogSync(e, "Failed to add server, is the address valid?");
            }
            return null;
        }

        [SmartAssembly.Attributes.ReportUsage]
        void CancelAddServer() {
            AddServerInput = null;
            IsAddServerVisible = false;
        }

        #region IHandle events

        public async void Handle(GameContentAfterSynced message) {
            // Already BG?
            if (IsActive)
                await _contentManager.InitialServerSync().ConfigureAwait(false);
        }

        public void Handle(GameLaunchedEvent message) {
            // Already BG?
            if (ServerList.IsUpdating && (_settings.AppOptions.SuspendSyncWhileGameRunning))
                Abort();
        }

        public void Handle(CalculatedGameSettingsUpdated message) {
            if (IsActive)
                CgsUpdate.Execute(null);
        }

        Task OnCgsUpdate(object x) {
            RefreshFilterCommand.Execute(null);
            return ServerList.UpdateServers();
        }

        public void Handle(ServersAdded message) {
            RefreshFilterCommand.Execute(null);
        }

        public void Handle(ServersUpdated message) {
            RefreshFilterCommand.Execute(null);
        }

        public void SwitchGame(Game game) {
            ResetCounts();
            HandleServerFilters(game);
            CreateLibrary(game);
        }

        #endregion
    }
}