// <copyright company="SIX Networks GmbH" file="GamesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Options;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    [DoNotObfuscate]
    public class GamesViewModel : ModuleViewModelBase, IHandle<RequestGameSettingsOverlay>,
        ISelectionList<GameDataModel>, IHaveSelectedItems
    {
        readonly IGameContext _gameContext;
        readonly GameInfoOverlayViewModel _gameInfoOverlay;
        readonly object _itemsLock = new object();
        readonly Lazy<LaunchManager> _launchManager;
        readonly IMediator _mediator;
        GameDataModel _activeGame;
        GameSettingsOverlayViewModel _gso;
        GameDataModel _selectedItem;
        IReactiveDerivedList<GameDataModel> _selectedItems;
        ObservableCollection<object> _selectedItemsInternal;
        protected GamesViewModel() {}

        public GamesViewModel(IMediator mediator,
            GameInfoOverlayViewModel giovm, IGameContext gameContext,
            UserSettings settings, Lazy<LaunchManager> launchManager) {
            _gameContext = gameContext;
            ModuleName = ControllerModules.GameBrowser;
            DisplayName = "Games";
            UserSettings = settings;
            _gameInfoOverlay = giovm;
            _mediator = mediator;
            _launchManager = launchManager;
            ContextMenu = new GameContextMenu(this);
            BarMenu = new GameBarMenu(this);
            this.SetCommand(x => x.DoubleClickedCommand);

            this.WhenAnyValue(x => x.SelectedItemsInternal)
                .Select(x => x == null ? null : x.CreateDerivedCollection(i => (GameDataModel) i))
                .BindTo(this, x => x.SelectedItems);

            ClearSelectionCommand = ReactiveUI.ReactiveCommand.Create();
            ClearSelectionCommand.Subscribe(x => ClearSelection());
        }

        public ReactiveCommand<object> ClearSelectionCommand { get; }
        public IReactiveDerivedList<GameDataModel> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }
        public GameBarMenu BarMenu { get; }
        public GameContextMenu ContextMenu { get; }
        public UserSettings UserSettings { get; }
        public ICollectionView View { get; private set; }
        public ReactiveCommand DoubleClickedCommand { get; private set; }
        public GameDataModel ActiveGame
        {
            get { return _activeGame; }
            set { SetProperty(ref _activeGame, value); }
        }
        // TODO: Services should be stateless; the stateful bits could be extracted to a separate private class?

        #region IHandle events

        public void Handle(RequestGameSettingsOverlay message) {
            Open();
            ShowSettings(message.GameId == Guid.Empty ? ActiveGame : Items.First(x => x.Id == message.GameId));
        }

        #endregion

        public ObservableCollection<object> SelectedItemsInternal
        {
            get { return _selectedItemsInternal; }
            set { SetProperty(ref _selectedItemsInternal, value); }
        }

        public void ClearSelection() {
            SelectedItemsInternal.Clear();
        }

        public ReactiveList<GameDataModel> Items { get; private set; }
        public GameDataModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            var items = _mediator.Request(new ListGamesQuery());
            items.EnableCollectionSynchronization(_itemsLock);
            Items = items;

            View = Items.SetupDefaultCollectionView(
                new[] {
                    new SortDescription("IsInstalled", ListSortDirection.Descending),
                    new SortDescription("IsFavorite", ListSortDirection.Descending),
                    new SortDescription("Name", ListSortDirection.Ascending)
                }, new[] {new PropertyGroupDescription("IsInstalled")},
                new[] {"IsInstalled"},
                Filter, true);

            DoubleClickedCommand.Cast<GameDataModel>().Subscribe(ActivateGame);

            DomainEvilGlobal.SelectedGame.WhenAnyValue(x => x.ActiveGame)
                .Subscribe(ActivateGame);

            this.WhenAnyValue(x => x.ActiveGame)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SelectedItem = x);

            this.WhenAnyValue(x => x.SelectedItem)
                .Where(x => x != null)
                .Subscribe(x => {
                    ContextMenu.ShowForItem(x);
                    BarMenu.ShowForItem(x);
                });

            if (!Execute.InDesignMode) {
                this.WhenAnyValue(x => x.IsActive)
                    .Skip(1)
                    .Subscribe(x => UsageCounter.ReportUsage("Tab Games"));
            }

            UserSettings.WhenAnyValue(x => x.Ready)
                .Subscribe(x => ProgressState.Active = !x);

            UserSettings.WhenAnyValue(x => x.GameOptions.GameSettingsController.ActiveProfile)
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => HandleSettings());

            this.WhenAnyValue(x => x.SelectedItem)
                .Skip(1)
                .Subscribe(HandleSettings);
        }

        void HandleSettings() {
            HandleSettings(SelectedItem);
        }

        void HandleSettings(GameDataModel x) {
            if (x == null)
                return;
            // TODO: HACK For game changing from installed to not installed, resetting selected item :S
            var gso = _gso;
            if (gso != null && gso.IsActive)
                ShowSettings(x);
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public bool ShopForMore() {
            // TODO: Integrated once we have a real buy page; atm it opens Play page and that means in client Stream without menu bar (with buy)
            return Tools.Generic.TryOpenUrl(CommonUrls.PlayUrl);
        }

        [DoNotObfuscate]
        public void ActivateItem(GameDataModel game) {
            ActivateGame(game);
        }

        void ActivateGame(Game game) {
            ActiveGame = Items.First(x => x.Id == game.Id);
        }

        void ActivateGame(GameDataModel game) {
            DomainEvilGlobal.SelectedGame.ActiveGame = _gameContext.Games.Find(game.Id);
        }

        [DoNotObfuscate]
        public Task LaunchNow(GameDataModel game) {
            ActivateGame(game);
            return _launchManager.Value.StartGame();
        }

        [DoNotObfuscate]
        public void UseGame(GameDataModel game) {
            ActivateGame(game);
        }

        [DoNotObfuscate]
        public void OpenGameFolder(GameDataModel game) {
            Tools.FileUtil.OpenFolderInExplorer(game.Directory);
        }

        [DoNotObfuscate]
        public void PurchaseGame(GameDataModel game) {
            var url = game.StoreUrl;
            if (url != null)
                Tools.Generic.TryOpenUrl(url);
        }

        /*        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void OpenNote(GameDataModel notes) {
            ShowNotes(_gameContext.Games.First(x => x.Id == notes.Id));
        }*/

        bool Filter(object item) {
            return true;
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void ShowSettings(GameDataModel game) {
            Contract.Requires<ArgumentNullException>(game != null);
            SelectedItem = game;
            if (_gso != null)
                _gso.TryClose();

            if (_gso == null || _gso.GameSettings.GameId != game.Id)
                _gso = _mediator.Request(new ShowGameSettingsQuery(game.Id));
            ShowOverlay(_gso);
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void ShowInfo(GameDataModel game) {
            Contract.Requires<ArgumentNullException>(game != null);
            SelectedItem = game;
            ShowOverlay(_gameInfoOverlay);
        }

        [DoNotObfuscate]
        public void DoubleClicked(RoutedEventArgs Args) {
            if (Args.FilterControlFromDoubleClick())
                return;

            var vm = Args.FindListBoxItem<GameDataModel>();
            if (vm != null)
                DoubleClickedCommand.Execute(vm);
            Args.Handled = true;
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void ShowSupport(GameDataModel game) {
            Tools.Generic.TryOpenUrl(game.SupportUrl);
        }
    }
}