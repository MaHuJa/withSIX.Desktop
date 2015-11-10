// <copyright company="SIX Networks GmbH" file="ModLibraryViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MoreLinq;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    [DoNotObfuscate]
    public class ModLibraryViewModel : ContentLibraryRootViewModel, IDropTarget, ITransient
    {
        readonly IContentManager _contentList;
        readonly Lazy<IContentManager> _contentManager;
        readonly SelectionCollectionHelper<MenuItem<ContentLibraryItemViewModel>> _defaultContextMenu =
            new SelectionCollectionHelper<MenuItem<ContentLibraryItemViewModel>>();
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly IGameLauncherFactory _gameLaunchFactory;
        readonly Lazy<LaunchManager> _launchManager;
        readonly IMediator _mediator;
        readonly ModsViewModel _modSetsViewModel;
        readonly ExportFactory<PickContactViewModel> _pickContactFactory;
        readonly UserSettings _settings;
        readonly object _setupLock = new object();
        readonly IUpdateManager _updateManager;
        bool _canUninstall = true;
        bool _canVerify = true;
        Game _game;
        ModLibrarySetup _librarySetup;

        public ModLibraryViewModel(ModsViewModel modSetsViewModel, IContentManager contentList,
            IEventAggregator eventBus, UserSettings settings, ExportFactory<PickContactViewModel> pickContactFactory,
            IGameLauncherFactory gameLaunchFactory, IMediator mediator, IDialogManager dialogManager,
            Lazy<LaunchManager> launchManager, Lazy<IContentManager> contentManager,
            UploadCollectionPopupMenu uploadCollectionMenu, CollectionSettingsMenu collectionSettingsMenu,
            UploadCollection uploadCollection, IUpdateManager updateManager)
            : base(pickContactFactory, modSetsViewModel) {
            UploadCollectionMenu = uploadCollectionMenu;
            CollectionSettingsMenu = collectionSettingsMenu;
            _modSetsViewModel = modSetsViewModel;
            _contentList = contentList;
            _eventBus = eventBus;
            _dialogManager = dialogManager;
            _gameLaunchFactory = gameLaunchFactory;
            _mediator = mediator;
            _launchManager = launchManager;
            _settings = settings;
            _pickContactFactory = pickContactFactory;
            _contentManager = contentManager;
            UploadCollection = uploadCollection;
            _updateManager = updateManager;

            SearchItem = new ModSearchContentLibraryItemViewModel(this);
            CollectionStateChangeMenu = new CollectionStateChangeMenu(uploadCollection);

            Comparer = new ModSearchItemComparer();

            this.SetCommand(x => x.ViewCategoryOnline).Cast<string>().Subscribe(ViewCatOnline);
            this.SetCommand(x => x.GetContentAddedCommand).Subscribe(GetContentAdded);
            this.SetCommand(x => x.UninstallCommand).RegisterAsyncTaskVoid<IContent>(Uninstall).Subscribe();
            this.SetCommand(x => x.RemoveCommand).RegisterAsyncTaskVoid<IContent>(RemoveContent).Subscribe();
            this.SetCommand(x => x.VerifyCommand).RegisterAsyncTaskVoid<IContent>(Verify).Subscribe();

            this.SetCommand(x => x.HandleModInCollectionCommand)
                .RegisterAsyncTaskVoid<IMod>(HandleModInCollection).Subscribe();

            this.SetCommand(x => x.ChangeScopeCommand)
                .Cast<CustomCollectionLibraryItemViewModel>()
                .Subscribe(ChangeScope);
            ReactiveUI.ReactiveCommand.CreateAsyncTask(
                x => ShowCollectionImageDialog((CustomCollectionLibraryItemViewModel) x))
                .SetNewCommand(this, x => x.ChangeImageCommand)
                .Subscribe();
            this.SetCommand(x => x.UploadCommand)
                .RegisterAsyncTaskVoid<CustomCollectionLibraryItemViewModel>(UploadCollection.Upload)
                .Subscribe();
            this.SetCommand(x => x.ShareCollectionCommand).Cast<CustomCollectionLibraryItemViewModel>().Subscribe(Share);
            this.SetCommand(x => x.SyncCollectionCommand)
                .RegisterAsyncTaskVoid<CollectionLibraryItemViewModel>(Sync)
                .Subscribe();
            this.SetCommand(x => x.UnsubscribeCollectionCommand)
                .RegisterAsyncTaskVoid<SubscribedCollectionLibraryItemViewModel>(Unsubscribe)
                .Subscribe();
            ReactiveUI.ReactiveCommand.CreateAsyncTask(x => OpenAddToCollectionsView(x as IMod))
                .SetNewCommand(this, x => x.AddToCollectionsCommand)
                .Subscribe();

            ViewType = settings.ModOptions.ViewType;
            this.ObservableForProperty(x => x.ViewType)
                .Select(x => x.Value)
                .BindTo(settings, s => s.ModOptions.ViewType);

            TreeModContextMenu = new ModContextMenu(this);
            CustomCollectionTreeContextMenu = new CustomCollectionContextMenu(this);
            RepositoryTreeContextMenu = new RepositoryOptionsContextMenu(this);
            LocalModFolderTreeContextMenu = new LocalModFolderContextMenu(this);
            BuiltInTreeContextMenu = new BuiltInContextMenu(this);

            MultiLibraryItemContextMenu = new MultiLibraryItemContextMenu(this);

            this.SetCommand(x => x.ToggleEnabled);
            ToggleEnabled.Subscribe(x => SelectedItem.FindItem<ToggleableModProxy>().ToggleEnabled());


            IsLoading = true;
        }

        protected ModLibraryViewModel() : base(null, null) {}
        public ReactiveCommand ToggleEnabled { get; private set; }
        public MultiLibraryItemContextMenu MultiLibraryItemContextMenu { get; }
        public CollectionStateChangeMenu CollectionStateChangeMenu { get; }
        public ReactiveCommand HandleModInCollectionCommand { get; private set; }
        public ReactiveCommand<Unit> AddToCollectionsCommand { get; private set; }
        public ReactiveCommand ChangeScopeCommand { get; private set; }
        public ReactiveCommand<Unit> ChangeImageCommand { get; private set; }
        public ReactiveCommand UploadCommand { get; private set; }
        public ReactiveCommand VerifyCommand { get; private set; }
        public ReactiveCommand RemoveCommand { get; private set; }
        public ReactiveCommand UninstallCommand { get; private set; }
        public ReactiveCommand DeleteCollectionCommand { get; private set; }
        public ReactiveCommand UnsubscribeCollectionCommand { get; private set; }
        public ReactiveCommand ShareCollectionCommand { get; private set; }
        public ReactiveCommand SyncCollectionCommand { get; private set; }
        ModLibrarySetup LibrarySetup
        {
            get { return _librarySetup; }
            set
            {
                _librarySetup = value;
                SetUp = value;
            }
        }
        public Game Game
        {
            get { return _game; }
            internal set { SetProperty(ref _game, value); }
        }
        public UploadCollectionPopupMenu UploadCollectionMenu { get; }
        public CollectionSettingsMenu CollectionSettingsMenu { get; }
        ModContextMenu TreeModContextMenu { get; }
        CustomCollectionContextMenu CustomCollectionTreeContextMenu { get; }
        BuiltInContextMenu BuiltInTreeContextMenu { get; }
        RepositoryOptionsContextMenu RepositoryTreeContextMenu { get; }
        LocalModFolderContextMenu LocalModFolderTreeContextMenu { get; }
        public bool CanVerify
        {
            get { return _canVerify; }
            set { SetProperty(ref _canVerify, value); }
        }
        public bool CanUninstall
        {
            get { return _canUninstall; }
            set { SetProperty(ref _canUninstall, value); }
        }
        public ReactiveCommand GetContentAddedCommand { get; private set; }
        public UploadCollection UploadCollection { get; }

        public void DragOver(IDropInfo dropInfo) {
            var targetItem = dropInfo.TargetItem as ContentLibraryItemViewModel<Collection>;
            if (!IsValidDropTarget(targetItem))
                return;

            var data = dropInfo.Data;
            if (!IsDroppable(data))
                return;
            //dropInfo.DropTargetAdorner = DropTargetAdorner
            dropInfo.Effects = DragDropEffects.Copy;
            dropInfo.DestinationText = "Add to " + targetItem.Model.Name;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
        }

        public void Drop(IDropInfo dropInfo) {
            if (dropInfo.Data == dropInfo.TargetItem)
                return;

            var targetItem = dropInfo.TargetItem as ContentLibraryItemViewModel<Collection>;
            if (dropInfo.TargetItem is SubscribedCollectionLibraryItemViewModel || targetItem == null)
                return;

            var targetCollection = targetItem.Model;
            var items = dropInfo.Data as IEnumerable;
            if (items != null)
                AddSourceItems(items.OfType<object>().ToArray(), targetCollection);
            else
                AddSourceItem(dropInfo.Data, targetCollection);
        }

        static bool IsValidDropTarget(ContentLibraryItemViewModel<Collection> targetItem) {
            return !(targetItem is SubscribedCollectionLibraryItemViewModel) && targetItem != null &&
                   targetItem.Model.Id != Guid.Empty;
        }

        static bool IsDroppable(object data) {
            return data is IContent || data is ContentLibraryItemViewModel<Collection> || HasAny<IContent>(data) ||
                   HasAny<ContentLibraryItemViewModel<Collection>>(data);
        }

        Task ShowCollectionImageDialog(CustomCollectionLibraryItemViewModel x) {
            return _dialogManager.ShowDialogAsync(_mediator.Request(new GetCollectionImageViewModelQuery(x.Model.Id)));
        }

        static bool HasAny<T>(object data) {
            var items = data as IEnumerable;
            return items != null && items.OfType<T>().Any();
        }

        void AddSourceItems(IReadOnlyCollection<object> objects, Collection targetCollection) {
            var mods = objects.OfType<IMod>().Select(x => x.ToMod()).ToArray();
            if (mods.Any())
                AddToModSet(targetCollection, mods);

            foreach (
                var item in
                    objects.OfType<Collection>()
                        .Concat(objects.OfType<ContentLibraryItemViewModel<Collection>>().Select(x => x.Model)))
                AddToModSet(targetCollection, item);
        }

        void AddSourceItem(object sourceContent, Collection targetCollection) {
            var sourceItem = sourceContent.ToMod();
            if (sourceItem != null)
                AddToModSet(targetCollection, sourceItem);
            else {
                var sourceModSet = sourceContent as Collection;
                if (sourceModSet != null)
                    AddToModSet(targetCollection, sourceModSet);
                else {
                    var sourceCollection = sourceContent as ContentLibraryItemViewModel<Collection>;
                    if (sourceCollection != null)
                        AddToModSet(targetCollection, sourceCollection.Model);
                }
            }
        }

        Task HandleModInCollection(IMod x) {
            if (ActiveItem == null)
                return OpenAddToCollectionsView(x);
            return x.IsInCurrentCollection
                ? Task.Run(() => RemoveFromCollection(x))
                : Task.Run(() => AddToCollection(x));
        }

        internal async Task OpenAddToCollectionsView(IReadOnlyCollection<IMod> mods) {
            throw new NotImplementedException();
            /*            var vm =
                await
                    _mediator.RequestAsync(new GetPickCollectionViewModelQuery(mods.Select(x => x.Id).ToArray(), Game.Id)).ConfigureAwait(false);
            _modSetsViewModel.ShowOverlay(vm);*/
        }

        internal async Task OpenAddToCollectionsView(IMod mod) {
            var vm =
                await
                    _mediator.RequestAsync(new GetPickCollectionViewModelQuery(mod.Id, Game.Id));
            _modSetsViewModel.ShowOverlay(vm);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WhenAnyValue(x => x.Game.CalculatedSettings.Collection)
                .Subscribe(x => ActiveItem = x);

            var ai = this.WhenAnyValue(x => x.ActiveItem);
            ai.Where(x => x != null)
                .OfType<Collection>()
                .Select(FindByModel)
                .Where(x => x != null)
                .Subscribe(x => SelectedItem = x);

            ai.Subscribe(
                modSet =>
                    UsageCounter.ReportUsage("Active ModSet: {0}".FormatWith(modSet == null ? "None" : modSet.Name)));

            ai
                .Skip(1)
                .Subscribe(OnActiveItemChanged);

            this.WhenAnyValue(x => x.ActiveItem).OfType<CustomCollection>()
                .Where(x => x.HasCustomRepo())
                .Subscribe(HandleCustomRepoDialog);

            this.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(item => {
                    HandleSingleMenu(item);
                    HandleActiveItem(item);
                });

            this.WhenAnyObservable(x => x.SelectedItems.ItemsAdded,
                x => x.SelectedItems.ItemsRemoved)
                .Select(_ => Unit.Default)
                .Merge(this.WhenAnyObservable(x => x.SelectedItems.ShouldReset).Select(_ => Unit.Default))
                .Select(x => SelectedItems)
                .Subscribe(x => {
                    switch (x.Count) {
                    case 0:
                        ContextMenu = null;
                        break;
                    case 1:
                        HandleSingleMenu(x.First());
                        break;
                    default:
                        HandleMultiMenu(x.ToArray());
                        break;
                    }
                });

            this.WhenAnyValue(x => x.SelectedItem.SelectedItem)
                .Where(x => x != null)
                .Subscribe(OnSelectedItem2Changed);
        }

        void HandleMultiMenu(IHierarchicalLibraryItem[] list) {
            if (!list.Any()) {
                ContextMenu = null;
                return;
            }
            MultiLibraryItemContextMenu.SetNextItem(list);
            ContextMenu = MultiLibraryItemContextMenu;
        }

        async void HandleCustomRepoDialog(CustomCollection x) {
            if (x.RememberWarn())
                return;
            await _dialogManager.ShowDialogAsync(new CustomRepoAvailabilityWarningViewModel(x));
        }

        void ChangeScope(CustomCollectionLibraryItemViewModel collection) {
            CollectionStateChangeMenu.SetNextItem(collection);
            CollectionStateChangeMenu.IsOpen = true;
        }

        public Task Unsubscribe(SubscribedCollectionLibraryItemViewModel collection) {
            return _mediator.RequestAsyncWrapped(new UnsubscribeFromCollectionCommand(collection.Model.Id));
        }

        [DoNotObfuscate]
        public void ViewOnline() {
            var item = SelectedItem as CollectionLibraryItemViewModel;
            if (item == null || !item.IsHosted)
                return;
            item.ViewOnline();
        }

        [DoNotObfuscate]
        public void VisitAuthorProfile() {
            var item = SelectedItem as CollectionLibraryItemViewModel;
            if (item == null || !item.IsHosted)
                return;
            item.VisitAuthorProfile();
        }

        [DoNotObfuscate]
        public override void DoubleClicked(RoutedEventArgs eventArgs) {
            if (eventArgs.FilterControlFromDoubleClick())
                return;

            var item = eventArgs.FindListBoxItem<IContent>();
            if (item != null)
                ItemDoubleclicked(item);
            else {
                var item2 = eventArgs.FindListBoxItem<IHierarchicalLibraryItem>();
                if (item2 != null)
                    SelectedItem = item2;
            }
        }

        void ItemDoubleclicked(IContent item) {
            var ai = ActiveItem as Collection;
            var mod = item as IMod;
            if (ai != null && mod != null)
                ai.AddModAndUpdateState(mod, _contentList);
            else
                ActiveItem = item;
        }

        [DoNotObfuscate]
        public override void DoubleClickedDG(RoutedEventArgs eventArgs) {
            if (eventArgs.FilterControlFromDoubleClick())
                return;

            var item = eventArgs.FindDataGridItem<IContent>();
            if (item != null)
                ItemDoubleclicked(item);
            else {
                var item2 = eventArgs.FindDataGridItem<IHierarchicalLibraryItem>();
                if (item2 != null)
                    SelectedItem = item2;
            }
        }

        void ViewCatOnline(string category) {
            if (category == null || category == "Unknown")
                return;

            var url = string.Format("http://play.withsix.com/{0}/mods/category/{1}", Game.MetaData.Slug,
                category.Sluggify());
            _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(url));
        }

        void AddToModSet(Collection targetCollection, IMod sourceItem) {
            if (!targetCollection.AddModAndUpdateStateIfPersistent(sourceItem, _contentList))
                CannotAddToModSetDialog(targetCollection);
            else if (targetCollection == ActiveItem)
                sourceItem.IsInCurrentCollection = true;
        }

        void AddToModSet(Collection targetCollection, IReadOnlyCollection<IMod> sourceItem) {
            if (!targetCollection.AddModAndUpdateStateIfPersistent(sourceItem, _contentList))
                CannotAddToModSetDialog(targetCollection);
            else if (targetCollection == ActiveItem) {
                foreach (var i in sourceItem)
                    i.IsInCurrentCollection = true;
            }
        }

        void AddToModSet(Collection targetCollection, Collection sourceCollection) {
            if (sourceCollection == targetCollection)
                return;
            IMod[] mods;
            lock (sourceCollection.Items)
                mods = sourceCollection.ModItems().ToArray();

            if (
                !_dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        String.Format("You are about to add {0} mods to {1} from {2} are you sure?",
                            mods.Length, targetCollection.Name, sourceCollection.Name),
                        "Adding mods from one collection to another", SixMessageBoxButton.YesNo)).IsYes())
                return;
            if (!targetCollection.AddModAndUpdateStateIfPersistent(mods, _contentList))
                CannotAddToModSetDialog(targetCollection);
            else if (targetCollection == ActiveItem)
                mods.ForEach(x => x.IsInCurrentCollection = true);
        }

        void CannotAddToModSetDialog(Collection targetCollection) {
            _dialogManager.MessageBoxSync(new MessageBoxDialogParams(GetAddMessage(targetCollection),
                "Cannot add to collection"));
        }

        static string GetAddMessage(Collection targetCollection) {
            var customModSet = targetCollection as CustomCollection;
            return customModSet != null && !customModSet.IsOpen
                ? "The mod(s) are either already part of the collection, or this is a custom repo collection that is 'Closed' (the hoster needs to 'Open' it to add mods)"
                : "The mod(s) are already part of the collection";
        }

        public async Task AddCollection(IContent content = null) {
            FindByModel(await _modSetsViewModel.CreateModSet(content).ConfigureAwait(false)).IsEditing = true;
        }

        public async Task AddCollection(IReadOnlyCollection<IContent> content) {
            FindByModel(await _modSetsViewModel.CreateModSet(content).ConfigureAwait(false)).IsEditing = true;
        }

        protected override void SuggestAddItems() {
            SelectNetwork();
        }

        public void SelectNetwork() {
            SelectedItem = LibrarySetup.Network;
        }

        async Task AddLocalFolder() {
            var path = await _dialogManager.BrowseForFolderAsync();
            if (string.IsNullOrWhiteSpace(path))
                return;

            await Task.Run(async () => {
                if (LibrarySetup.GetLocalMods().Any(x => Tools.FileUtil.ComparePathsOsCaseSensitive(x.Model.Path, path))) {
                    await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("You've already added this folder"));
                    return;
                }

                var item = LibrarySetup.CreateLocalItem(Path.GetFileName(path), false, path);
                _contentList.LocalModsContainers.Add(item.Model);
                DomainEvilGlobal.Settings.RaiseChanged();
            });
        }

        [DoNotObfuscate]
        public void ShowSettings() {
            ShowSettings(SelectedItem.FindItem<IContent>());
        }

        [DoNotObfuscate]
        public void ShowSettings(IContent mod) {
            _modSetsViewModel.ModSetConfigure(null);
        }

        [DoNotObfuscate]
        public void ShowInfo() {
            ShowInfo(SelectedItem.FindItem<IContent>());
        }

        void ShowModInfo(IMod mod) {
            if (AllowInfoOverlay(mod))
                ShowInfoOverlay(mod);
            else
                BrowserHelper.TryOpenUrlIntegrated(mod.ProfileUrl());
        }

        static bool AllowInfoOverlay(IMod mod) {
            return mod is CustomRepoMod || mod is LocalMod;
        }

        [DoNotObfuscate]
        public void ShowInfo(IContent mod) {
            SelectedItem.SelectedItem = mod;
            var m = mod.ToMod();
            if (m != null)
                ShowModInfo(m);
            else
                ShowInfoOverlay(mod);
        }

        [DoNotObfuscate]
        public void ShowVersion(IContent mod) {
            SelectedItem.SelectedItem = mod;
            _modSetsViewModel.ModVersion();
        }

        void ShowInfoOverlay(IContent mod) {
            _modSetsViewModel.ModSetInfo();
        }

        [DoNotObfuscate]
        public void AddToCollection(IContent mod) {
            var ms = ActiveItem as Collection;
            if (ms == null)
                return;

            var m = mod.ToMod();
            if (m != null)
                AddToModSet(ms, m);

            var mss = mod as Collection;
            if (mss != null)
                AddToModSet(ms, mss);
        }

        public void AddToCollection(IReadOnlyCollection<IContent> content) {
            var ms = ActiveItem as Collection;
            if (ms == null)
                return;

            var mods = content.OfType<IMod>().Select(x => x.ToMod()).ToArray();
            if (mods.Any())
                AddToModSet(ms, mods);

            foreach (var item in content.OfType<Collection>())
                AddToModSet(ms, item);
        }

        [DoNotObfuscate]
        public void RemoveFromCollection(IContent mod) {
            var ms = ActiveItem as Collection;
            if (ms == null)
                return;

            RemoveFromCollection(mod, ms);
        }

        [DoNotObfuscate]
        public void RemoveFromCollection(IReadOnlyCollection<IContent> content) {
            var ms = ActiveItem as Collection;
            if (ms == null)
                return;

            RemoveFromCollection(content, ms);
        }

        public void RemoveFromSelectedCollection(IContent mod) {
            RemoveFromCollection(mod, (SelectedItem as ContentLibraryItemViewModel<Collection>).Model);
        }

        void RemoveFromCollection(IReadOnlyCollection<IContent> content, Collection ms) {
            var mods = content.OfType<IMod>().Select(x => x.ToMod()).ToArray();
            RemoveFromModSet(ms, mods);

            var collections = content.OfType<Collection>().ToArray();
            RemoveFromModSet(ms, collections);
        }

        void RemoveFromModSet(Collection collection, IEnumerable<Collection> collections) {
            var mods = collections.SelectMany(x => x.ModItems()).ToArray();
            if (!mods.Any())
                return;
            if (!collection.RemoveModAndUpdateStateIfPersistent(mods, _contentList))
                CannotRemoveFromModSetDialog(collection);
            else if (collection == ActiveItem)
                mods.ForEach(x => x.IsInCurrentCollection = false);
        }

        void RemoveFromModSet(Collection collection, IReadOnlyCollection<IMod> mods) {
            if (!mods.Any())
                return;
            if (!collection.RemoveModAndUpdateStateIfPersistent(mods, _contentList))
                CannotRemoveFromModSetDialog(collection);
            else if (collection == ActiveItem) {
                foreach (var m in mods)
                    m.IsInCurrentCollection = false;
            }
        }

        void RemoveFromCollection(IContent mod, Collection ms) {
            var m = mod.ToMod();
            if (m != null)
                RemoveFromModSet(ms, m);

            var mss = mod as Collection;
            if (mss != null)
                RemoveFromModSet(ms, mss);
        }

        void RemoveFromModSet(Collection collection, IMod mod) {
            if (!collection.RemoveModAndUpdateStateIfPersistent(mod, _contentList))
                CannotRemoveFromModSetDialog(collection);
            else if (collection == ActiveItem)
                mod.IsInCurrentCollection = false;
        }

        void RemoveFromModSet(Collection collection, Collection sourceCollection) {
            var mods = sourceCollection.ModItems().ToArray();
            if (!collection.RemoveModAndUpdateStateIfPersistent(mods, _contentList))
                CannotRemoveFromModSetDialog(collection);
            else if (collection == ActiveItem)
                mods.ForEach(x => x.IsInCurrentCollection = false);
        }

        void CannotRemoveFromModSetDialog(Collection targetCollection) {
            _dialogManager.MessageBoxSync(new MessageBoxDialogParams(GetRemoveMessage(targetCollection),
                "Cannot remove from collection"));
        }

        static string GetRemoveMessage(Collection targetCollection) {
            var customModSet = targetCollection as CustomCollection;
            return customModSet != null && !customModSet.IsOpen
                ? "The mod(s) are either not part of the collection (e.g dependency or server required mod),\nor this is a custom repo collection that is 'Closed' (the hoster needs to 'Open' it to remove mods)"
                : "The mod(s) are not part of the collection (e.g dependency or server required mod)";
        }

        [DoNotObfuscate]
        public async void RemoveLibraryItem() {
            var item = SelectedItem.FindItem<ContentLibraryItemViewModel>();
            if (item != null)
                await RemoveLibraryItem(item).ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public override async Task RemoveLibraryItem(ContentLibraryItemViewModel content) {
            var repo = content as ContentLibraryItemViewModel<SixRepo>;
            if (repo != null) {
                RemoveSixRepo(repo.Model);
                return;
            }

            var localMods = content as ContentLibraryItemViewModel<LocalModsContainer>;
            if (localMods != null) {
                RemoveLocalMods(localMods.Model);
                return;
            }

            var modSet = content as ContentLibraryItemViewModel<Collection>;
            if (modSet != null)
                await RemoveCollection(modSet.Model).ConfigureAwait(false);
        }

        public Task RemoveLibraryItem(IEnumerable<CollectionLibraryItemViewModel> items) {
            var customCollections = items.Select(x => x.Model).OfType<CustomCollection>().ToArray();
            if (customCollections.Any() &&
                !_dialogManager.MessageBox(
                    new MessageBoxDialogParams("Are you sure you want to remove selected collections?",
                        "Remove selected collections", SixMessageBoxButton.YesNo)).IsYes())
                return Task.FromResult(0);
            foreach (var collection in customCollections)
                _contentManager.Value.RemoveCollection(collection);

            if (items.Select(x => x.Model).OfType<SubscribedCollection>().Any()) {
                _dialogManager.MessageBox(
                    new MessageBoxDialogParams("To remove a Subscribed Collection please Unsubscribe from it"));
            }
            return Task.FromResult(0);
        }

        void Share(CustomCollectionLibraryItemViewModel collection) {
            UploadCollection.ShowCollectionCreatedDialog(collection);
        }

        async Task Sync(CollectionLibraryItemViewModel collection) {
            var custom = collection as CustomCollectionLibraryItemViewModel;
            if (custom == null) {
                await RefreshCustomRepoInfo((CustomRepoCollectionLibraryItemViewModel) collection).ConfigureAwait(false);
                return;
            }

            try {
                await UploadCollection.Sync(custom).ConfigureAwait(false);
            } catch (AlreadyExistsException ex) {
                // TODO: Handle this better...
                this.Logger()
                    .WarnException("The collection synchronization did not occur, because there are no changes", ex);
            }
        }

        public Task RemoveCollection(Collection collection) {
            var cc = collection as CustomCollection;
            if (cc != null)
                return RemoveCollection(cc);
            var sc = collection as SubscribedCollection;
            if (sc != null)
                return RemoveCollection(sc);

            throw new NotSupportedException("This collection type is not supported");
        }

        [SmartAssembly.Attributes.ReportUsage]
        public Task RemoveCollection(CustomCollection collection) {
            Contract.Requires<ArgumentNullException>(collection != null);
            if (collection.PublishedId != null)
                return Remove(collection);
            return Task.Run(() => {
                var report =
                    _dialogManager.MessageBoxSync(
                        new MessageBoxDialogParams("Do you want to remove:\n" + collection.Name,
                            "Are you sure about removing the modset?", SixMessageBoxButton.YesNo)).IsYes();

                UsageCounter.ReportUsage("Dialog - Remove ModSet {0}".FormatWith(report));

                if (report) {
                    _contentManager.Value.RemoveCollection(collection);
                    return true;
                }
                return false;
            });
        }

        public async Task Remove(CustomCollection item) {
            if (
                (await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        "Are you sure you want to delete this synced collection? This cannot be undone",
                        "Delete synced collection?", SixMessageBoxButton.YesNo))).IsYes())
                await _mediator.RequestAsyncWrapped(new DeleteCollectionCommand(item.Id));
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task RemoveCollection(SubscribedCollection collection) {
            _dialogManager.MessageBox(
                new MessageBoxDialogParams("To remove a Subscribed Collection please Unsubscribe from it"));
        }

        [DoNotObfuscate]
        public async Task Verify(IContent mod) {
            CanVerify = false;
            try {
                var modSet = mod as Collection;
                if (modSet == null)
                    await ModVerify(mod.ToMod()).ConfigureAwait(false);
                else
                    await ModSetVerify(modSet).ConfigureAwait(false);
            } finally {
                CanVerify = true;
            }
        }

        public async Task RemoveContent(IContent mod) {
            var modSet = mod as CustomCollection;
            if (modSet != null)
                await RemoveCollection(modSet).ConfigureAwait(false);
        }

        public async Task Uninstall(IContent content) {
            CanUninstall = false;
            try {
                var modSet = content as Collection;
                var modi = content.ToMod();
                if (modSet != null)
                    await _modSetsViewModel.UninstallModSet(modSet).ConfigureAwait(false);
                else if (modi != null)
                    await _modSetsViewModel.ModUninstall(modi).ConfigureAwait(false);
            } finally {
                CanUninstall = true;
            }
        }

        public async Task Uninstall(IReadOnlyCollection<IContent> content) {
            foreach (var content1 in content)
                await Uninstall(content1).ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public void ShowNotes(IHaveNotes content) {
            _modSetsViewModel.OpenNote(content);
        }

        public async Task CreateShortcutGame(Collection collection) {
            var description = String.Format("Active mods: {0}\nCreated:{1}",
                string.Join(", ", collection.EnabledMods.Select(x => x.Name)), DateTime.Now);
            var shortcutLaunchParameters =
                await
                    ((IShortcutCreation) Game).ShortcutLaunchParameters(_gameLaunchFactory,
                        Tools.HashEncryption.MD5Hash(description)).ConfigureAwait(false);
            await ShortcutCreator.CreateDesktopGameBat(
                String.Format(@"Play {0}_{1}", Game.MetaData.Name, collection.Name),
                description,
                shortcutLaunchParameters.CombineParameters(), Game,
                await _modSetsViewModel.CreateIcon(collection).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        public Task CreateShortcutPws(Collection collection) {
            return ShortcutCreator.CreateDesktopPwsIcon(
                String.Format(@"Play {0}_{1} withSIX", Game.MetaData.Name, collection.Name),
                String.Format("ModSet: {0}\nCreated:{1}", collection.Name, DateTime.Now),
                string.Format("pws://?mod_set={0}&action=update,launch", collection.Id));
        }

        public Task CreateShortcutPwsLockdown(Collection collection) {
            return ShortcutCreator.CreateDesktopPwsIcon(
                String.Format(@"Play {0}_{1} withSIX (lockdown)", Game.MetaData.Name, collection.Name),
                String.Format("ModSet: {0}\nCreated:{1}", collection.Name, DateTime.Now),
                string.Format("pws://?mod_set={0}&lockdown=true", collection.Id));
        }

        public void CloneCollection(ContentLibraryItemViewModel<Collection> content) {
            var current = content.Model;
            var ms = _contentList.CloneCollection(current);
            ActiveItem = ms;
        }

        public Task Diagnose(IContent content) {
            var modset = content as Collection;
            if (modset != null)
                return ModSetVerify(modset);
            var mod = content.ToMod();
            if (mod != null)
                return ModVerify(mod);

            throw new Exception("Unsupported content");
        }

        public Task Diagnose(IReadOnlyCollection<IContent> content) {
            return ModSetVerify(CreateTemporaryCollection(content));
        }

        Collection CreateTemporaryCollection(IReadOnlyCollection<IContent> content) {
            var coll = new Collection(Guid.NewGuid(), Game.Modding());
            foreach (var content1 in content) {
                if (content1 is IMod)
                    coll.AddModAndUpdateState(content1.ToMod(), _modSetsViewModel.ContentManager);
            }
            return coll;
        }

        public async Task ShareToContact(IReadOnlyCollection<IContent> content) {
            using (var vm = _pickContactFactory.CreateExport()) {
                await vm.Value.Load(CreateTemporaryCollection(content));
                // UI stuff
                vm.Value.SetCurrent(null);
                _modSetsViewModel.ShowOverlay(vm.Value);
            }
        }

        public Task LaunchCollection(ContentLibraryItemViewModel<Collection> getLibraryItem) {
            return Launch(getLibraryItem.Model);
        }

        public void UninstallCollection(ContentLibraryItemViewModel<Collection> getLibraryItem) {
            Uninstall(getLibraryItem.Model);
        }

        public Task Uninstall(Collection collection) {
            return _modSetsViewModel.UninstallModSet(collection);
        }

        public void ShowCollectionNotes(ContentLibraryItemViewModel<Collection> getLibraryItem) {
            _modSetsViewModel.OpenNote(getLibraryItem.Model);
        }

        public Task Launch(IContent content) {
            ActiveItem = content;
            return _launchManager.Value.StartGame();
        }

        public Task Launch(IReadOnlyCollection<IContent> content) {
            ActiveItem = CreateTemporaryCollection(content);
            return _launchManager.Value.StartGame();
        }

        public void MoveLocalModDirectory(ContentLibraryItemViewModel<LocalModsContainer> getLibraryItem) {
            _eventBus.PublishOnUIThread(new RequestGameSettingsOverlay(Game.Id));
        }

        void GetContentAdded() {
            BrowserHelper.TryOpenUrlIntegrated(
                "https://community.withsix.com/topic/2/arma-mod-requests");
        }

        protected override bool ApplySearchFilter(object obj) {
            var content = obj as IContent;
            var mod = content.ToMod();
            if (mod is CustomRepoMod)
                return false;

            if (mod != null) {
                ApplyScore(mod);
                return mod.SearchScore != -1;
            }

            var item = content as Content;
            return item != null && item.Name.NullSafeContainsIgnoreCase(SearchText);
        }

        void ApplyScore(IMod mod) {
            mod.SearchScore = Score(mod);
        }

        int Score(IMod mod) {
            var i = mod.Name.NullSafeIndexOfIgnoreCase(SearchText);
            if (i > -1)
                return 100000 - i - (mod.Name.Length - SearchText.Length);

            i = mod.FullName.NullSafeIndexOfIgnoreCase(SearchText);
            if (i > -1)
                return 100000 - i - (mod.FullName.Length - SearchText.Length);

            i = mod.Author.NullSafeIndexOfIgnoreCase(SearchText);
            if (i > -1)
                return 90000 - i - (mod.Author.Length - SearchText.Length);

            if (mod.Categories.Any(x => x.NullSafeContainsIgnoreCase(SearchText)))
                return 80;

            if (mod.Dependencies.Any(x => x.NullSafeContainsIgnoreCase(SearchText)))
                return 70;

            return -1;
        }

        [DoNotObfuscate]
        public void ActivateSelectedItem() {
            ActiveItem = ((CollectionLibraryItemViewModel) SelectedItem).Model;
        }

        public override sealed void Setup() {
            lock (_setupLock) {
                IsLoading = true;
                ItemsView = null;
                var previous = LibrarySetup;
                LibrarySetup = new ModLibrarySetup(this, Game, _contentList, _settings, _eventBus);
                LibrarySetup.Updates.Items.CountChanged.Where(x => x == 0).Subscribe(x => {
                    if (ActiveItem == LibrarySetup.Updates.Model)
                        ActiveItem = null;
                });
                SetupGroups();
                InitialSelectedItem();

                // TODO: This might be another cause for nullref excpetion in WPF binding framework...
                try {
                    if (previous == null)
                        return;
                    previous.Dispose();
                } finally {
                    IsLoading = false;
                }
            }
        }

        protected override void InitialSelectedItem() {
            var ai = ActiveItem as Collection;
            if (ai != null)
                SelectedItem = FindByModel(ai) ?? LibrarySetup.Items.FirstOrDefault();
            else
                SelectedItem = LibrarySetup.Items.FirstOrDefault();
        }

        ContentLibraryItemViewModel FindByModel(Collection ai) {
            return FindByModel<ContentLibraryItemViewModel<Collection>, Collection>(ai);
        }

        void SetupGroupDescriptions() {
            var descs = ItemsView.GroupDescriptions[0];
            LibrarySetup.Groups.SyncCollection(descs.GroupNames);
        }

        void OnActiveItemChanged(IContent content) {
            var modSet = content as Collection;
            var mod = content.ToMod();
            if (mod != null) {
                modSet = _contentList.CreateCustomCollection(Game.Modding(), mod);
                modSet.Name = mod.Name;
            }
            Game.CalculatedSettings.Collection = modSet;
            UsageCounter.ReportUsage("Changed active modset");

            if (modSet == null) {
                foreach (var i in _contentList.Mods.ToArrayLocked())
                    i.IsInCurrentCollection = false;
                return;
            }

            var mods = modSet.ModItems().ToArrayLocked();
            foreach (var i in _contentList.Mods.ToArrayLocked())
                i.IsInCurrentCollection = mods.Contains(i);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public Task ModVerify(IMod mod) {
            // Workaround for no custom repo traveling with the mod into the new blank collection :)
            var collectionLibraryItem = SelectedItem as CustomCollectionLibraryItemViewModel;
            if (collectionLibraryItem != null) {
                var collection = collectionLibraryItem.Model;
                if (collection != null) {
                    var col = _contentList.CreateCustomCollection(Game.Modding(), mod, collection);
                    col.Name = mod.Name;
                    ActiveItem = col;
                    return ModSetVerify(col);
                }
            }
            ActiveItem = mod;
            return ModSetVerify(DomainEvilGlobal.SelectedGame.ActiveGame.CalculatedSettings.Collection);
        }

        [SmartAssembly.Attributes.ReportUsage]
        async Task ModSetVerify(Collection collection) {
            ActiveItem = collection;
            try {
                await _updateManager.HandleConvertOrInstallOrUpdate(true).ConfigureAwait(false);
            } catch (BusyStateHandler.BusyException) {
                _dialogManager.BusyDialog();
            }
        }

        public override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
            base.OnSelectedItemChanged(e);
            var mod = e.NewValue.ToMod();
            if (mod == null)
                return;
            TreeModContextMenu.SetNextItem(mod);
            ContextMenu = TreeModContextMenu;
        }

        void OnSelectedItem2Changed(IHierarchicalLibraryItem x) {
            var mod = x.ToMod();
            if (mod != null)
                HandleOverlay(mod);
        }

        void HandleOverlay(IMod mod) {
            if (_modSetsViewModel.ModSetInfoOverlay.IsActive && !(AllowInfoOverlay(mod)))
                _modSetsViewModel.ModSetInfoOverlay.TryClose();

            if (_modSetsViewModel.ModSetInfoOverlay.IsActive &&
                !mod.Controller.HasMultipleVersions())
                _modSetsViewModel.ModSetInfoOverlay.TryClose();
        }

        void HandleActiveItem(IHierarchicalLibraryItem x) {
            var col = x as CollectionLibraryItemViewModel;
            if (col != null)
                ActiveItem = col.Model;
        }

        void HandleSingleMenu(IHierarchicalLibraryItem x) {
            if (x == null) {
                ContextMenu = null;
                return;
            }
            // TODO: Just store the menus on the libraryitemviewmodels ??
            var col = x as CustomCollectionLibraryItemViewModel;
            if (col != null) {
                CollectionSettingsMenu.SetNextItem(col);
                UploadCollectionMenu.SetNextItem(col);
                CustomCollectionTreeContextMenu.SetNextItem(col);
                ContextMenu = CustomCollectionTreeContextMenu;
                return;
            }

            var ms = x as CollectionLibraryItemViewModel;
            if (ms != null) {
                CollectionSettingsMenu.SetNextItem(ms);
                CustomCollectionTreeContextMenu.SetNextItem(ms);
                ContextMenu = CustomCollectionTreeContextMenu;
                return;
            }

            var r = x as ContentLibraryItemViewModel<SixRepo>;
            if (r != null) {
                RepositoryTreeContextMenu.SetNextItem(r);
                ContextMenu = RepositoryTreeContextMenu;
                return;
            }

            var lmod = x as ContentLibraryItemViewModel<LocalModsContainer>;
            if (lmod != null) {
                LocalModFolderTreeContextMenu.SetNextItem(lmod);
                ContextMenu = LocalModFolderTreeContextMenu;
                return;
            }

            var bcc = x as ContentLibraryItemViewModel<BuiltInContentContainer>;
            if (bcc != null) {
                BuiltInTreeContextMenu.SetNextItem(bcc);
                ContextMenu = BuiltInTreeContextMenu;
                return;
            }

            /*
            var c = x as IMod;
            if (c != null) {
                TreeModContextMenu.SetNextItem(c);
                ContextMenu = TreeModContextMenu;
                return;
            }
*/

            ContextMenu = null;
        }

        void SetupGroups() {
            LibrarySetup.CollectionsGroup.AddCommand.RegisterAsyncTask(_modSetsViewModel.ShowCreateCollection)
                .Subscribe();
            LibrarySetup.LocalGroup.AddCommand.RegisterAsyncTask(AddLocalFolder).Subscribe();
            LibrarySetup.OnlineGroup.AddCommand.RegisterAsyncTask(_modSetsViewModel.ShowAddRepository).Subscribe();
        }

        void RemoveLocalMods(LocalModsContainer localMods) {
            _contentList.LocalModsContainers.Remove(localMods);
            DomainEvilGlobal.Settings.RaiseChanged();
            localMods.Dispose();
        }

        void RemoveSixRepo(SixRepo repo) {
            _contentList.CustomRepositories.Remove(repo);
            var repositories = _settings.ModOptions.Repositories;
            foreach (var v in repositories.Where(x => x.Value.Equals(repo)).ToArray())
                repositories.Remove(v.Key);

            var activeModSet = Game.CalculatedSettings.Collection as CustomCollection;
            if (activeModSet != null && activeModSet.CustomRepo == repo)
                Game.CalculatedSettings.Collection = null;

            _contentList.CustomCollections.RemoveAll(x => x.CustomRepo == repo);
            DomainEvilGlobal.Settings.RaiseChanged();
        }

        public Task RefreshCustomRepoInfo(CollectionLibraryItemViewModel getLibraryItem) {
            return _contentManager.Value.RefreshCollectionInfo(getLibraryItem.Model);
        }

        public void ClearCollection(ContentLibraryItemViewModel<Collection> getLibraryItem) {
            if (
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams("You are about to clear the collection of all mods, are you sure?",
                        "Clear collection?", SixMessageBoxButton.YesNo)).IsYes())
                getLibraryItem.Model.Clear(_contentList);
        }

        public void ClearCollections(IEnumerable<ContentLibraryItemViewModel<Collection>> items) {
            if (
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "You are about to clear the selected collections of all mods, are you sure?",
                        "Clear selected collections?", SixMessageBoxButton.YesNo)).IsYes())
                items.ForEach(x => x.Model.Clear(_contentList));
        }

        public void ClearCustomizations(ContentLibraryItemViewModel<Collection> getLibraryItem) {
            if (
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "You are about to clear the collection of customizations, are you sure?",
                        "Clear collection of customizations?", SixMessageBoxButton.YesNo)).IsYes())
                getLibraryItem.Model.ClearCustomizations(_contentList);
        }

        public void ClearCustomizations(IEnumerable<ContentLibraryItemViewModel<Collection>> items) {
            if (
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "You are about to clear the selected collections of customizations, are you sure?",
                        "Clear selected collections of customizations?", SixMessageBoxButton.YesNo)).IsYes())
                items.ForEach(x => x.Model.ClearCustomizations(_contentList));
        }

        [DoNotObfuscate]
        public Task CreateShortcutPwsJoin(Collection collection) {
            return ShortcutCreator.CreateDesktopPwsIcon(
                String.Format(@"Play {0}_{1} withSIX", Game.MetaData.Name, collection.Name),
                String.Format("ModSet: {0}\nCreated:{1}", collection.Name, DateTime.Now),
                string.Format("pws://?mod_set={0}&action=update,join", collection.Id));
        }

        [DoNotObfuscate]
        public void ShowDependency(string dependency) {
            BrowserHelper.TryOpenUrlIntegrated(Tools.Transfer.JoinUri(CommonUrls.PlayUrl, Game.MetaData.Slug,
                "mods",
                dependency.Sluggify()));
        }

        public void Reset() {
            var librarySetup = _librarySetup;
            if (librarySetup == null)
                return;
            IsLoading = true;
            try {
                librarySetup.Reset();
            } finally {
                IsLoading = false;
            }
        }

        public Task ChangeCollectionScope(Guid id, CollectionScope collectionScope) {
            return _mediator.RequestAsyncWrapped(new ChangeCollectionScopeCommand(id, collectionScope));
        }
    }
}