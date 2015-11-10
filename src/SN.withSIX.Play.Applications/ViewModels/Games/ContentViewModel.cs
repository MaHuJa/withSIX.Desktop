// <copyright company="SIX Networks GmbH" file="ContentViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;
using IScreen = ReactiveUI.IScreen;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    [DoNotObfuscate]
    public class ContentViewModel : ReactiveConductor<ModuleViewModelBase>.Collection.OneActive, IContentViewModel,
        IHandle<RequestShowMissionLibrary>, IHaveOverlay
    {
        readonly IDialogManager _dialogManager;
        [DoNotObfuscate] readonly ObservableAsPropertyHelper<OverlayViewModelBase> _overlay;
        GameViewModel _activeGame;
        ExportLifetimeContext<GameViewModel> _activeGameContext;
        bool _isMenuOpen = true;
        int? _isServerAppsEnabled = 0;
        RepoAppsContextMenu _repoAppsContextMenu;

        public ContentViewModel(IPlayShellViewModel shellVM, GamesViewModel games, ServersViewModel servers,
            MissionsViewModel missions, ModsViewModel mods, HomeViewModel home, IUpdateManager updateManager,
            Func<RepoAppsContextMenu> contextMenu,
            IViewModelFactory factory, IDialogManager dialogManager) {
            _dialogManager = dialogManager;
            HostScreen = shellVM;
            Games = games;
            Servers = servers;
            Missions = missions;
            Mods = mods;
            Home = home;
            UpdateManager = updateManager;
            Factory = factory;

            RepoAppsContextMenu = contextMenu();

            Activator = new ViewModelActivator();

            _overlay = this.WhenAnyValue(x => x.ActiveItem.Overlay.ActiveItem)
                .ToProperty(this, x => x.Overlay, null, Scheduler.Immediate);

            var once = false;
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ActiveItem.DisplayName)
                    .BindTo(this, x => x.DisplayName));

                d(Games.DoubleClickedCommand.Subscribe(GameBrowserDoubleClicked));

                d(UpdateManager.StateChange.ObserveOn(RxApp.MainThreadScheduler).Subscribe(data => {
                    var customModSet = data.Item1 as CustomCollection;
                    RepoAppsContextMenu.Refresh(customModSet);
                    if (((IIsEmpty) RepoAppsContextMenu).IsEmpty())
                        IsServerAppsEnabled = 0;
                    else
                        IsServerAppsEnabled = null;
                }));

                d(DomainEvilGlobal.SelectedGame.WhenAnyValue(x => x.ActiveGame)
                    .Subscribe(GameChanged));
                /*            this.WhenAnyValue(x => x.ActiveGame.Mods.LibraryVM.ActiveItem)
                            .Where(x => x != null)
                            .Subscribe(ActiveItemChanged);*/
                d(ReactiveCommand.Create().SetNewCommand(this, x => x.SwitchMenuOpen)
                    .Select(_ => !IsMenuOpen)
                    .BindTo(this, x => x.IsMenuOpen));

                var observable = shellVM.WhenAnyValue(x => x.GridMode)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value ? ViewType.Grid : ViewType.List);

                d(observable.BindTo(Missions, x => x.LibraryVM.ViewType));
                d(observable.BindTo(Mods, x => x.LibraryVM.ViewType));
                d(observable.BindTo(Servers, x => x.LibraryVM.ViewType));

                if (once)
                    return;
                InitializeModules();
                once = true;
            });
        }

        public RepoAppsContextMenu RepoAppsContextMenu
        {
            get { return _repoAppsContextMenu; }
            set { SetProperty(ref _repoAppsContextMenu, value); }
        }
        public int? IsServerAppsEnabled
        {
            get { return _isServerAppsEnabled; }
            private set
            {
                if (value != 0)
                    SetProperty(ref _isServerAppsEnabled, null);
                else
                    SetProperty(ref _isServerAppsEnabled, value);
            }
        }
        public IViewModelFactory Factory { get; }
        public ServersViewModel Servers { get; }
        public MissionsViewModel Missions { get; }
        public ModsViewModel Mods { get; }
        public HomeViewModel Home { get; }
        public IUpdateManager UpdateManager { get; set; }
        public GameViewModel ActiveGame
        {
            get { return _activeGame; }
            set { SetProperty(ref _activeGame, value); }
        }
        public ViewModelActivator Activator { get; }
        public ReactiveCommand<object> SwitchMenuOpen { get; private set; }
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set { SetProperty(ref _isMenuOpen, value); }
        }

        public void GoGames() {
            ActivateItem(Games);
        }

        public void GoHome() {
            ActivateItem(Home);
        }

        public GamesViewModel Games { get; }
        public string UrlPathSegment
        {
            get { return "Content"; }
        }
        public IScreen HostScreen { get; }

        public void CloseOverlay() {
            ((IPlayShellViewModel) HostScreen).CloseOverlay();
        }

        /*
        void ActiveItemChanged(IContent content) {
            HandleTitle(content);
        }

        void HandleTitle(IContent content) {
            if (Common.Flags.LockDown)
                DisplayName = GetTitle(content.Name);
        }*/

        public void Handle(RequestShowMissionLibrary message) {
            ActiveGame.ShowMissionList();
        }

        public OverlayViewModelBase Overlay
        {
            get { return _overlay.Value; }
        }

        void InitializeModules() {
            // We would prefer to drop Home all together but then it's difficult to show it up top :)
            Items.Add(Home);
            PreInit(Games);
            PreInit(Mods);
            PreInit(Missions);
            PreInit(Servers);

            //ActivateLastModule();
            if (Common.Flags.LockDown) {
                var activeGame = ActiveGame;
                if (activeGame.Servers != null)
                    ActivateModule(ControllerModules.ServerBrowser);
            } else {
                // TODO: Also use RX Router instead
                ActivateModule(ControllerModules.Home);
            }

            /*
             * TODO: Store per game?
            this.WhenAnyValue(x => x.CurrentModule)
                .Skip(1)
                .Subscribe(
                    x => { _userSettings.AppOptions.LastModule = x == null ? null : x.ModuleName.ToString(); });
             */

            ActiveGame.InitModules();
        }

        void ActivateLastModule() {
            ControllerModules mod;
            if (Enum.TryParse(DomainEvilGlobal.Settings.AppOptions.LastModule, out mod))
                ActivateModule(mod);
        }

        void ActivateModule(ControllerModules module) {
            switch (module) {
            case ControllerModules.Home: {
                ActivateItem(Home);
                break;
            }
            case ControllerModules.GameBrowser: {
                ActivateItem(Games);
                break;
            }
            case ControllerModules.ModBrowser: {
                ActivateItem(ActiveGame.Mods);
                break;
            }
            case ControllerModules.ServerBrowser: {
                ActivateItem(ActiveGame.Servers);
                break;
            }
            case ControllerModules.MissionBrowser: {
                ActivateItem(ActiveGame.Missions);
                break;
            }
            }
        }

        void PreInit(ModuleViewModelBase module) {
            ActivateItem(module);
            ((IActivate) module).Activate();
            ((IDeactivate) module).Deactivate(false);
        }

        [ReportUsage]
        void GameBrowserDoubleClicked(object g) {
            var mods = ActiveGame.Mods;
            if (mods != null)
                ActivateItem(mods);
        }

        void GameChanged(Game x) {
            var previousContext = _activeGameContext;
            _activeGameContext = Factory.CreateGame(x);

            ActiveGame = x == null ? null : _activeGameContext.Value;

            if (previousContext != null)
                previousContext.Dispose();
        }
    }

    public interface IContentViewModel : IViewModel, IRoutableViewModel, IConductActiveItem, ISupportsActivation
    {
        ReactiveCommand<object> SwitchMenuOpen { get; }
        GamesViewModel Games { get; }
        bool IsMenuOpen { get; set; }
        void CloseOverlay();
        void GoGames();
        void GoHome();
    }
}