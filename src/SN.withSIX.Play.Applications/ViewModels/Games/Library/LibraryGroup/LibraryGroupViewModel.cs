// <copyright company="SIX Networks GmbH" file="LibraryGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup
{
    public abstract class LibraryGroupViewModel : LibraryBase, IHierarchicalLibraryItem
    {
        readonly object _childrenLock = new object();
        ICollectionView _childrenView;
        IContextMenu _contextMenu;
        bool _isExpanded;
        ICollectionView _itemsView;
        IHierarchicalLibraryItem _selectedItem;
        IReactiveDerivedList<IHierarchicalLibraryItem> _selectedItems;
        ObservableCollection<object> _selectedItemsInternal;

        protected LibraryGroupViewModel(string header, string addHeader = null, string icon = null) {
            Header = header;
            AddHeader = addHeader;
            Icon = icon;
            if (!Execute.InDesignMode)
                this.SetCommand(x => x.AddCommand);

            Children = new ReactiveList<IHierarchicalLibraryItem>();
            IsExpanded = true;

            this.WhenAnyValue(x => x.SelectedItemsInternal)
                .Select(x => x == null ? null : x.CreateDerivedCollection(i => (IHierarchicalLibraryItem) i))
                .BindTo(this, x => x.SelectedItems);


            UiHelper.TryOnUiThread(() => {
                Children.EnableCollectionSynchronization(_childrenLock);
                _childrenView =
                    Children.CreateCollectionView(
                        new[] {
                            new SortDescription("SortOrder", ListSortDirection.Ascending),
                            new SortDescription("Model.IsFavorite", ListSortDirection.Descending),
                            new SortDescription("Model.Name", ListSortDirection.Ascending)
                        }, null,
                        null, null, true);
                _itemsView = _childrenView;
            });
        }

        public IContextMenu ContextMenu
        {
            get { return _contextMenu; }
            protected set { SetProperty(ref _contextMenu, value); }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
        public string Icon { get; set; }
        public string AddHeader { get; set; }
        public ReactiveCommand AddCommand { get; protected set; }
        public bool IsSelected { get; set; }
        // TODO
        public IReactiveDerivedList<IHierarchicalLibraryItem> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }
        public override ICollectionView ItemsView
        {
            get { return _itemsView; }
        }
        public override ICollectionView ChildrenView
        {
            get { return _childrenView; }
        }
        public override IHierarchicalLibraryItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        public override ReactiveList<IHierarchicalLibraryItem> Children { get; }
        public override ObservableCollection<object> SelectedItemsInternal
        {
            get { return _selectedItemsInternal; }
            set { SetProperty(ref _selectedItemsInternal, value); }
        }
        object IHaveSelectedItem.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (IHierarchicalLibraryItem) value; }
        }
    }

    public abstract class LibraryGroupViewModel<T> : LibraryGroupViewModel where T : LibraryRootViewModel
    {
        readonly T _library;

        protected LibraryGroupViewModel(T library, string header, string addHeader = null, string icon = null)
            : base(header, addHeader, icon) {
            _library = library;
        }
    }
}