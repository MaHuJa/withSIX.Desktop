// <copyright company="SIX Networks GmbH" file="ContentLibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using ReactiveUI;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public abstract class ContentLibraryItemViewModel : LibraryItemViewModel<IContent>
    {
        protected static readonly PropertyGroupDescription PropertyGroupDescription =
            new PropertyGroupDescription("Categories");
        protected static readonly SortData[] RequiredColumns = {
            new SortData {
                DisplayName = "IsFavorite",
                Value = "IsFavorite",
                SortDirection =
                    ListSortDirection.Descending
            }
            /*
            new SortData {
                DisplayName = "IsRequired",
                Value = "IsRequired",
                SortDirection =
                    ListSortDirection.Descending
            }
*/
        };
        protected static readonly SortData[] Columns = {
            new SortData {
                DisplayName = "Name",
                Value = "FullName",
                SortDirection =
                    ListSortDirection.Ascending
            },
            new SortData {
                DisplayName = "Package/Folder Name",
                Value = "Name",
                SortDirection =
                    ListSortDirection.Ascending
            },
            new SortData {
                DisplayName = "Date Updated",
                Value = "UpdatedVersion",
                SortDirection =
                    ListSortDirection.Descending
            },
            new SortData {
                DisplayName = "Date Added",
                Value = "CreatedAt",
                SortDirection =
                    ListSortDirection.Descending
            }
        };
        readonly LibraryRootViewModel _library;

        protected ContentLibraryItemViewModel(LibraryRootViewModel library, LibraryGroupViewModel @group) : base(@group) {
            _library = library;
        }

        protected void SetupGrouping() {
            this.WhenAnyValue(x => x.DoGrouping)
                .Skip(1)
                .Subscribe(x => {
                    if (x)
                        EnableGrouping();
                    else
                        DisableGrouping();
                });
        }

        void EnableGrouping() {
            ItemsView.GroupDescriptions.AddWhenMissing(PropertyGroupDescription);
        }

        void DisableGrouping() {
            ItemsView.GroupDescriptions.Remove(PropertyGroupDescription);
        }

        public virtual Task Remove() {
            return _library.RemoveLibraryItem(this);
        }
    }

    public abstract class ContentLibraryItemViewModel<T> : ContentLibraryItemViewModel, IHaveModel<T>
        where T : class, ISelectionList<IContent>
    {
        readonly LibraryRootViewModel _library;
        bool _hasItems;

        protected ContentLibraryItemViewModel(LibraryRootViewModel library, T model, string icon = null,
            LibraryGroupViewModel @group = null, bool isFeatured = false, bool doGrouping = false)
            : this(library, model, doGrouping, group) {
            Icon = icon;
            Group = group;
            IsFeatured = isFeatured;
            Items.CountChanged.Subscribe(x => HasItems = x > 0);
        }

        ContentLibraryItemViewModel(LibraryRootViewModel library, T model, bool doGrouping = false,
            LibraryGroupViewModel group = null)
            : base(library, group) {
            _library = library;

            Model = model;
            DoGrouping = doGrouping;

            var groups = doGrouping
                ? new[] {PropertyGroupDescription}
                : null;

            Filter = new ModLibraryFilter();
            SetupFilterChanged();

            UiHelper.TryOnUiThread(() => {
                Items.EnableCollectionSynchronization(ItemsLock);
                _itemsView =
                    Items.CreateCollectionView(
                        new[] {
                            new SortDescription("IsFavorite", ListSortDirection.Descending),
                            //new SortDescription("IsRequired", ListSortDirection.Descending),
                            new SortDescription("FullName", ListSortDirection.Ascending)
                        },
                        groups, null, Filter.Handler, true);

                _childrenView =
                    Children.CreateCollectionView(
                        new[] {
                            new SortDescription("Model.IsFavorite", ListSortDirection.Descending),
                            new SortDescription("Model.Name", ListSortDirection.Ascending)
                        }, null,
                        null, null, true);
            });
            Sort = new SortViewModel(ItemsView, _columns, null, RequiredColumns);
            SetupGrouping();
        }

        protected virtual SortData[] _columns
        {
            get { return Columns; }
        }
        public bool HasItems
        {
            get { return _hasItems; }
            set { SetProperty(ref _hasItems, value); }
        }
        public override ReactiveList<IContent> Items
        {
            get { return Model.Items; }
        }
        public T Model { get; }
    }

    public class BrowseContentLibraryItemViewModel<T> : ContentLibraryItemViewModel<T>
        where T : class, ISelectionList<IContent>
    {
        readonly CollectionBarMenu _collectionBarMenu;
        readonly CollectionContextMenu _collectionContextMenu;
        readonly ModBarMenu _modBarMenu;
        readonly ModContextMenu _modContextMenu;
        readonly MultiContentContextMenu _multiContentContextMenu;

        public BrowseContentLibraryItemViewModel(ModLibraryViewModel library, T model, string icon = null,
            LibraryGroupViewModel @group = null, bool isFeatured = false,
            bool doGrouping = false)
            : base(library, model, icon, @group, isFeatured, doGrouping) {
            _collectionContextMenu = new CollectionContextMenu(library);
            _modContextMenu = new ModContextMenu(library);
            _multiContentContextMenu = new MultiContentContextMenu(library);
            _modBarMenu = new ModBarMenu(library);
            _collectionBarMenu = new CollectionBarMenu(library);

            SetupMenus(HandleSingleMenu, HandleMultiMenu);
        }

        void HandleMultiMenu(IReadOnlyCollection<IContent> items) {
            _multiContentContextMenu.ShowForItem(items);
            ContextMenu = _multiContentContextMenu;
        }

        void HandleSingleMenu(IContent first) {
            var mod = first as IMod;
            if (mod != null) {
                _modContextMenu.ShowForItem(mod);
                ContextMenu = _modContextMenu;
                _modBarMenu.ShowForItem(mod);
                BarMenu = _modBarMenu;
            } else {
                var collection = first as Collection;
                if (collection != null) {
                    _collectionContextMenu.ShowForItem(collection);
                    _collectionBarMenu.ShowForItem(collection);
                }
                ContextMenu = _collectionContextMenu;
                BarMenu = _collectionBarMenu;
            }
        }
    }

    public class MissionContentLibraryItemViewModel<T> : ContentLibraryItemViewModel<T>
        where T : class, ISelectionList<IContent>
    {
        public MissionContentLibraryItemViewModel(MissionLibraryViewModel library, T model, string icon = null,
            MissionLibraryGroupViewModel @group = null, bool isFeatured = false,
            bool doGrouping = false)
            : base(library, model, icon, @group, isFeatured, doGrouping) {
            MissionContextMenu = new MissionContextMenu(library);
            MissionFolderContextMenu = new MissionFolderContextMenu(library);
            MissionBarMenu = new MissionContextMenu(library);
            MissionFolderBarMenu = new MissionFolderContextMenu(library);

            SetupMenus(HandleSingleMenu, x => ContextMenu = null);
        }

        public MissionFolderContextMenu MissionFolderBarMenu { get; }
        public MissionContextMenu MissionBarMenu { get; }
        public MissionFolderContextMenu MissionFolderContextMenu { get; }
        public MissionContextMenu MissionContextMenu { get; }

        void HandleSingleMenu(IContent first) {
            var folder = first as MissionFolder;
            if (folder != null) {
                MissionFolderContextMenu.ShowForItem(folder);
                MissionFolderBarMenu.ShowForItem(folder);
                ContextMenu = MissionFolderContextMenu;
                BarMenu = MissionFolderBarMenu;
            } else {
                var mission = first as Mission;
                if (mission != null) {
                    MissionContextMenu.ShowForItem(mission);
                    MissionBarMenu.ShowForItem(mission);
                }
                BarMenu = MissionBarMenu;
                ContextMenu = MissionContextMenu;
            }
        }
    }

    public class CollectionLibraryItemViewModel : ContentLibraryItemViewModel<Collection>
    {
        static readonly SortData[] myColumns = Columns.Concat(new[] {
            new SortData {
                DisplayName = "Launch Order",
                Value = "Order",
                SortDirection =
                    ListSortDirection.Ascending
            }
        }).ToArray();
        readonly CollectionBarMenu _collectionBarMenu;
        readonly CollectionContextMenu _collectionContextMenu;
        readonly ModBarMenu _modBarMenu;
        readonly ModContextMenu _modContextMenu;
        readonly MultiContentContextMenu _multiContentContextMenu;

        public CollectionLibraryItemViewModel(ModLibraryViewModel library, Collection model, string icon,
            LibraryGroupViewModel @group = null, bool isFeatured = false,
            bool doGrouping = false)
            : base(library, model, icon, @group, isFeatured, doGrouping) {
            MainIcon = SixIconFont.withSIX_icon_Folder;
            ShowItemsInTree = true;
            Items.KeepCollectionInSync2(Children);


            _collectionContextMenu = new CollectionContextMenu(library);
            _modContextMenu = new ModContextMenu(library);
            _multiContentContextMenu = new MultiContentContextMenu(library);
            _modBarMenu = new ModBarMenu(library);
            _collectionBarMenu = new CollectionBarMenu(library);

            SetupMenus(HandleSingleMenu, HandleMultiMenu);
        }

        protected override SortData[] _columns
        {
            get { return myColumns; }
        }
        public virtual bool IsHosted
        {
            get { return false; }
        }
        public virtual bool IsPublishable
        {
            get { return false; }
        }
        public virtual bool IsPublished
        {
            get { return false; }
        }
        public virtual bool IsSubscribedCollection
        {
            get { return false; }
        }
        public virtual bool IsShareable
        {
            get { return false; }
        }
        public bool IsEditable { get; protected set; }

        public override Task Remove() {
            throw new NotSupportedException("This type of collection cannot be removed");
        }

        void HandleMultiMenu(IReadOnlyCollection<IContent> items) {
            _multiContentContextMenu.ShowForItem(items);
            ContextMenu = _multiContentContextMenu;
        }

        void HandleSingleMenu(IContent first) {
            var mod = first as IMod;
            if (mod != null) {
                _modContextMenu.ShowForItem(mod);
                ContextMenu = _modContextMenu;
                _modBarMenu.ShowForItem(mod);
                BarMenu = _modBarMenu;
            } else {
                var collection = first as Collection;
                if (collection != null) {
                    _collectionContextMenu.ShowForItem(collection);
                    _collectionBarMenu.ShowForItem(collection);
                }
                ContextMenu = _collectionContextMenu;
                BarMenu = _collectionBarMenu;
            }
        }

        public void ViewOnline() {
            BrowserHelper.TryOpenUrlIntegrated(Model.ProfileUrl());
        }

        public virtual void VisitAuthorProfile() {}
    }

    public class CustomCollectionLibraryItemViewModel : CollectionLibraryItemViewModel
    {
        protected readonly ObservableAsPropertyHelper<bool> _canLockCollection;
        protected readonly ObservableAsPropertyHelper<bool> _canUnlockCollection;
        // Protected because of obfuscation issues..
        protected readonly ObservableAsPropertyHelper<bool> _isHosted;
        protected readonly ObservableAsPropertyHelper<bool> _isListed;
        protected readonly ObservableAsPropertyHelper<bool> _isPublishable;
        protected readonly ObservableAsPropertyHelper<bool> _isPublished;
        protected readonly ObservableAsPropertyHelper<bool> _isSharable;
        readonly ModLibraryViewModel _library;
        bool _isPrivate;

        public CustomCollectionLibraryItemViewModel(ModLibraryViewModel library, CustomCollection model,
            LibraryGroupViewModel localGroup,
            bool isFeatured = false,
            bool doGrouping = false)
            : base(library, model, SixIconFont.withSIX_icon_Hexagon, localGroup, isFeatured, doGrouping) {
            _library = library;
            Model = model;
            IsEditable = !isFeatured;

            /*
            // TODO: How it really should be:
            // Or use a variation of Can....
            // this would be most appropriate for a ViewModel. For a DataModel it reduces the re-usability. But these things should be custom built for each View/Control anyway?
            _isPublishShown;
            _isDeleteShown;
            _isUploadShown;
            _isDeleteShown;
            _isMoreShown;
            _isChangeScopeShown;
            _isSubscribersShown;
            _showSyncedTime;
            */

            _isPublishable = model.WhenAnyValue(x => x.PublishedId)
                .Select(x => !x.HasValue)
                .ToProperty(this, x => x.IsPublishable);

            _isHosted = model.WhenAnyValue(x => x.PublishedId, x => x.HasValue)
                .ToProperty(this, x => x.IsHosted);

            _isListed = model.WhenAnyValue(x => x.PublishingScope, x => x == CollectionScope.Public)
                .ToProperty(this, x => x.IsListed);

            model.WhenAnyValue(x => x.PublishingScope)
                .Subscribe(x => IsPrivate = x == CollectionScope.Private);

            this.WhenAnyValue(x => x.IsHosted)
                .Subscribe(x => Icon = x ? SixIconFont.withSIX_icon_Synq : SixIconFont.withSIX_icon_Hexagon);

            _isPublished = model.WhenAnyValue(x => x.PublishedId, id => id.HasValue)
                .ToProperty(this, x => x.IsPublished);

            _isSharable = this.WhenAnyValue(x => x.IsPublished, x => x.IsPrivate,
                (isPublished, isPrivate) => isPublished && !isPrivate)
                .ToProperty(this, x => x.IsShareable);

            _canLockCollection = this.WhenAnyValue(x => x.IsSubscribedCollection)
                .Select(x => !x)
                .ToProperty(this, x => x.CanNotLockCollection);
        }

        public bool IsPrivate
        {
            get { return _isPrivate; }
            set { SetProperty(ref _isPrivate, value); }
        }
        public override bool IsShareable
        {
            get { return _isSharable.Value; }
        }
        public new CustomCollection Model { get; }
        public override bool IsPublished
        {
            get { return _isPublished.Value; }
        }
        public override bool IsHosted
        {
            get { return _isHosted.Value; }
        }
        public bool IsListed
        {
            get { return _isListed.Value; }
        }
        public override bool IsPublishable
        {
            get { return _isPublishable.Value; }
        }
        public bool CanNotLockCollection
        {
            get { return _canLockCollection.Value; }
        }

        public override Task Remove() {
            return _library.RemoveContent(Model);
        }

        public override void VisitAuthorProfile() {
            BrowserHelper.TryOpenUrlIntegrated(Model.GetAuthorUri());
        }
    }

    public class CustomRepoCollectionLibraryItemViewModel : CollectionLibraryItemViewModel
    {
        public CustomRepoCollectionLibraryItemViewModel(ModLibraryViewModel library, CustomCollection model,
            LibraryGroupViewModel @group = null,
            bool isFeatured = false, bool doGrouping = false)
            : base(library, model, SixIconFont.withSIX_icon_Synq, @group, isFeatured, doGrouping) {
            Model = model;
            IsEditable = false;
            SubHeader = model.CustomRepo.Name;
        }

        public override bool IsHosted
        {
            get { return true; }
        }
        public new CustomCollection Model { get; }

        public override void VisitAuthorProfile() {
            var authorUri = Model.GetAuthorUri();
            if (authorUri == null)
                return;
            BrowserHelper.TryOpenUrlIntegrated(authorUri);
        }
    }

    public class SubscribedCollectionLibraryItemViewModel : CollectionLibraryItemViewModel
    {
        readonly ModLibraryViewModel _library;

        public SubscribedCollectionLibraryItemViewModel(ModLibraryViewModel library, SubscribedCollection model,
            LibraryGroupViewModel @group = null,
            bool isFeatured = false, bool doGrouping = false)
            : base(library, model, SixIconFont.withSIX_icon_Synq, @group, isFeatured, doGrouping) {
            MainIcon = SixIconFont.withSIX_icon_Folder;
            _library = library;
            Model = model;
            IsEditable = false;

            SubHeader = model.Author;
        }

        public new SubscribedCollection Model { get; }
        public override bool IsSubscribedCollection
        {
            get { return true; }
        }
        public override bool IsPublishable
        {
            get { return false; }
        }
        public override bool IsHosted
        {
            get { return true; }
        }

        public override Task Remove() {
            return _library.RemoveLibraryItem(this);
        }

        public override void VisitAuthorProfile() {
            BrowserHelper.TryOpenUrlIntegrated(Model.GetAuthorUri());
        }
    }

    public class LocalModsLibraryItemViewModel : BrowseContentLibraryItemViewModel<LocalModsContainer>
    {
        public LocalModsLibraryItemViewModel(ModLibraryViewModel library, LocalModsContainer model,
            LibraryGroupViewModel @group = null,
            bool isFeatured = false, bool doGrouping = false)
            : base(library, model, SixIconFont.withSIX_icon_Folder, @group, isFeatured, doGrouping) {
            Items.ChangeTrackingEnabled = true;
            Description = model.Path;
        }
    }

    public class NetworkLibraryItemViewModel : BrowseContentLibraryItemViewModel<BuiltInContentContainer>
    {
        public NetworkLibraryItemViewModel(ModLibraryViewModel library, BuiltInContentContainer model,
            LibraryGroupViewModel @group = null,
            bool doGrouping = false)
            : base(library, model, SixIconFont.withSIX_icon_Nav_Server, @group, true, doGrouping) {
            Items.ChangeTrackingEnabled = true;
            var currentSort = Sort.SelectedSort;
            if (currentSort == null || currentSort.Value != "UpdatedVersion")
                Sort.SetSort("UpdatedVersion");
        }
    }

    /*    public class DesignTimeCustomCollectionLibraryItem : CustomCollectionLibraryItem
        {
            const string DescriptionText =
                "I’m Brick Tamland. People seem to like me because I am polite and I am rarely late. I like to eat ice cream and I really enjoy a nice pair of slacks. Years later, a doctor will tell me that I have an I.Q. of 48 and am what some people call mentally retarded.";

            public DesignTimeCustomCollectionLibraryItem()
                : base(                new CustomCollection(Guid.NewGuid(), new Arma1Game(Guid.NewGuid(), new GameSettingsController())) {
                        Name = "Test Collection",
                        Author = "Oliver Baker",
                        CreatedAt = DateTime.Now,
                        Description = DescriptionText
                    }, new ModLibraryGroup(this, "Some group", null, SixIconFont.withSIX_icon_Folder)) {}
        }*/
}