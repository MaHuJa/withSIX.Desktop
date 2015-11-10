// <copyright company="SIX Networks GmbH" file="LibraryItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public static class HierarchicalExtensions
    {
        public static T FindItem<T>(this IHierarchicalLibraryItem hi) where T : class {
            while (hi != null) {
                var c = hi as T;
                if (c != null)
                    return c;
                hi = hi.SelectedItem;
            }
            return null;
        }
    }

    public abstract class LibraryBase : ReactiveValidatableObjectBase, IHierarchicalLibraryItem
    {
        string _header;

        protected LibraryBase() {
            ClearSelectionCommand = ReactiveCommand.Create();
            ClearSelectionCommand.Subscribe(x => ClearSelection());
        }

        public string Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }
        public int SortOrder { get; set; } = 100;
        public bool IsHead { get; set; }
        public bool IsRoot { get; set; }
        public IReactiveCommand<object> ClearSelectionCommand { get; }
        public abstract ObservableCollection<object> SelectedItemsInternal { get; set; }

        public void ClearSelection() {
            SelectedItemsInternal.Clear();
        }

        public abstract ReactiveList<IHierarchicalLibraryItem> Children { get; }
        public abstract ICollectionView ChildrenView { get; }
        public abstract IHierarchicalLibraryItem SelectedItem { get; set; }
        object IHaveSelectedItem.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (IHierarchicalLibraryItem) value; }
        }
        public abstract ICollectionView ItemsView { get; }
    }

    public abstract class LibraryItemViewModel : LibraryBase, ISelectable, IHaveSelectedItem
    {
        readonly object _childrenLock = new object();
        [DoNotObfuscate] readonly ObservableAsPropertyHelper<bool> _hasChildren;
        IContextMenu _barMenu;
        protected ICollectionView _childrenView;
        IContextMenu _contextMenu;
        string _description;
        bool _doGrouping;
        LibraryGroupViewModel _group;
        string _icon;
        string _iconForeground;
        bool _isEditing;
        bool _isEditingDescription;
        bool _isFeatured;
        bool _isSelected;
        protected ICollectionView _itemsView;
        string _mainIcon;
        IHierarchicalLibraryItem _selectedItem;
        ObservableCollection<object> _selectedItemsInternal;
        bool _showItemsInTree;
        string _subHeader;

        protected LibraryItemViewModel(LibraryGroupViewModel @group) {
            Children = new ReactiveList<IHierarchicalLibraryItem>();
            Group = group;
            if (!Execute.InDesignMode) {
                _hasChildren = Children.CountChanged
                    .Select(x => x > 0).ToProperty(this, x => x.HasChildren, false, Scheduler.Immediate);
            }
            UiHelper.TryOnUiThread(() => Children.EnableCollectionSynchronization(_childrenLock));
        }

        public bool ShowItemsInTree
        {
            get { return _showItemsInTree; }
            set { SetProperty(ref _showItemsInTree, value); }
        }
        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }
        public bool IsEditingDescription
        {
            get { return _isEditingDescription; }
            set { SetProperty(ref _isEditingDescription, value); }
        }
        public IFilter Filter { get; protected set; }
        public SortViewModel Sort { get; protected set; }
        public override ICollectionView ChildrenView
        {
            get { return _childrenView; }
        }
        public override ICollectionView ItemsView
        {
            get { return _itemsView; }
        }
        public string SubHeader
        {
            get { return _subHeader; }
            set { SetProperty(ref _subHeader, value); }
        }
        public string Icon
        {
            get { return _icon; }
            set
            {
                if (!SetProperty(ref _icon, value))
                    return;
                OnPropertyChanged("MainIcon");
            }
        }
        public string IconForeground
        {
            get { return _iconForeground; }
            set { SetProperty(ref _iconForeground, value); }
        }
        public string MainIcon
        {
            get { return _mainIcon ?? Icon ?? Group.Icon; }
            set { _mainIcon = value; }
        }
        public LibraryGroupViewModel Group
        {
            get { return _group; }
            protected set { SetProperty(ref _group, value); }
        }
        public virtual bool HasChildren
        {
            get { return _hasChildren.Value; }
        }
        public override ReactiveList<IHierarchicalLibraryItem> Children { get; }
        public override IHierarchicalLibraryItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        public bool DoGrouping
        {
            get { return _doGrouping; }
            set { SetProperty(ref _doGrouping, value); }
        }
        public bool IsFeatured
        {
            get { return _isFeatured; }
            set { SetProperty(ref _isFeatured, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        public IContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { SetProperty(ref _contextMenu, value); }
        }
        public IContextMenu BarMenu
        {
            get { return _barMenu; }
            set { SetProperty(ref _barMenu, value); }
        }
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
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }

    public abstract class LibraryItemViewModel<T> : LibraryItemViewModel, IHaveSelectedItemsView<T>
        where T : class, IHierarchicalLibraryItem
    {
        protected readonly object ItemsLock = new object();
        IReactiveDerivedList<T> _selectedItems;

        protected LibraryItemViewModel(LibraryGroupViewModel group) : base(group) {
            this.WhenAnyValue(x => x.SelectedItemsInternal)
                .Select(x => x == null ? null : x.CreateDerivedCollection(i => (T) i))
                .BindTo(this, x => x.SelectedItems);
        }

        public IReactiveDerivedList<T> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }
        public new T SelectedItem
        {
            get { return (T) base.SelectedItem; }
            set { base.SelectedItem = value; }
        }
        public abstract ReactiveList<T> Items { get; }
        object IHaveSelectedItem.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (T) value; }
        }

        protected void SetupMenus(Action<T> singleMenu, Action<IReadOnlyCollection<T>> multiMenu) {
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
                        singleMenu(x.First());
                        break;
                    default:
                        multiMenu(x.ToArray());
                        break;
                    }
                });
            /*
            this.WhenAnyValue(x => x.SelectedItem)
                .DistinctUntilChanged()
                .Subscribe(x => {
                    if (x == null)
                        ContextMenu = null;
                    else
                        singleMenu(x);
                });*/
        }

        protected void SetupFilterChanged() {
            Filter.FilterChanged
                .Throttle(Common.AppCommon.DefaultFilterDelay, RxApp.MainThreadScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ItemsView.TryRefreshIfHasView());
        }
    }
}