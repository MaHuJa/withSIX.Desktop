// <copyright company="SIX Networks GmbH" file="LibraryRootViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public interface ILibraryViewModel
    {
        bool NavigationVisible { get; set; }
    }

    public abstract class LibraryRootViewModel : ReactiveScreen
    {
        [DoNotObfuscate]
        public virtual void DoubleClickedDG(RoutedEventArgs args) {}

        [DoNotObfuscate]
        public virtual void DoubleClicked(RoutedEventArgs args) {}

        public abstract Task RemoveLibraryItem(ContentLibraryItemViewModel contentLibraryItemViewModel);
    };


    public interface IHaveSelectedItemsView<T> : IHaveSelectedItemsView, IHaveSelectedItem<T>, ISelectionList<T>
        where T : class {}

    [DoNotObfuscate]
    public abstract class LibraryRootViewModel<T, T2, T3> : LibraryRootViewModel,
        IHaveSelectedItem<IHierarchicalLibraryItem>,
        ILibraryViewModel, IHaveSelectedItems
        where T : LibraryItemViewModel<T2>
        where T2 : class, ISearchScore, IToggleFavorite, IHierarchicalLibraryItem
        where T3 : T, ISearchLibraryItem<T2>
    {
        readonly object _searchLock = new object();
        T2 _activeItem;
        IContextMenu _contextMenu;
        bool _isLoading = true;
        ICollectionView _itemsView;
        bool _navigationVisible = true;
        LibraryItemViewModel _oldSelectedItem;
        string _searchText;
        IHierarchicalLibraryItem _selectedItem;
        IReactiveDerivedList<IHierarchicalLibraryItem> _selectedItems;
        ObservableCollection<object> _selectedItemsInternal;
        LibrarySetup<T> _setUp;
        SortViewModel _sort;
        ViewType _viewType;
        protected IEqualityComparer<T2> Comparer;
        protected T3 SearchItem;

        protected LibraryRootViewModel() {
            if (Execute.InDesignMode)
                return;

            this.WhenAnyValue(x => x.SetUp)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(() => this.WhenAnyValue(x => x.SearchText)
                    .Throttle(Common.AppCommon.DefaultFilterDelay, RxApp.MainThreadScheduler)
                    .Scan(new {cts = new CancellationTokenSource(), searchText = default(string)},
                        (previous, newObj) => {
                            try {
                                previous.cts.Cancel();
                            } catch (ObjectDisposedException) {}
                            // TODO: Improve; exceptions aren't great for performance
                            return new {cts = new CancellationTokenSource(), searchText = newObj};
                        })
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .Subscribe(s => {
                        try {
                            HandleSearch(s.searchText, s.cts.Token);
                        } finally {
                            s.cts.Dispose();
                        }
                    }));

            var selectedItemObservable = this.WhenAnyValue(x => x.SelectedItem).OfType<LibraryItemViewModel>();

            selectedItemObservable
                .Subscribe(x => {
                    var oldSelectedItem = _oldSelectedItem;
                    if (oldSelectedItem != null) {
                        oldSelectedItem.IsEditing = false;
                        oldSelectedItem.IsEditingDescription = false;
                    }
                    _oldSelectedItem = x;
                });

            selectedItemObservable.Where(x => x != null)
                .OfType<ISelectable>()
                .Subscribe(x => x.IsSelected = true);

            this.SetCommand(x => x.ResetFiltersCommand).Subscribe(() => {
                var i = SelectedItem.FindItem<ServerLibraryItemViewModel>();
                if (i != null)
                    i.Filter.ResetFilter();
            });

            this.WhenAnyValue(x => x.SetUp.ItemsView)
                .Subscribe(x => ItemsView = x);

            this.SetCommand(x => x.ToggleFavorite);
            ToggleFavorite.Subscribe(x => SelectedItem.FindItem<IToggleFavorite>().ToggleFavorite());

            ViewTypeObservable = this.WhenAnyValue(x => x.ViewType)
                .Select(x => x.ToString());

            this.WhenAnyValue(x => x.SelectedItemsInternal)
                .Select(x => x == null ? null : x.CreateDerivedCollection(i => (IHierarchicalLibraryItem) i))
                .BindTo(this, x => x.SelectedItems);
        }

        public IReactiveDerivedList<IHierarchicalLibraryItem> SelectedItems
        {
            get { return _selectedItems; }
            private set { SetProperty(ref _selectedItems, value); }
        }
        public IObservable<string> ViewTypeObservable { get; private set; }
        public ReactiveCommand ToggleFavorite { get; private set; }
        public LibrarySetup<T> SetUp
        {
            get { return _setUp; }
            set { SetProperty(ref _setUp, value); }
        }
        public ReactiveCommand ResetFiltersCommand { get; set; }
        public ReactiveCommand ViewCategoryOnline { get; set; }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }
        public ICollectionView ItemsView
        {
            get { return _itemsView; }
            set { SetProperty(ref _itemsView, value); }
        }
        public T2 ActiveItem
        {
            get { return _activeItem; }
            set { SetProperty(ref _activeItem, value); }
        }
        public SortViewModel Sort
        {
            get { return _sort; }
            set { SetProperty(ref _sort, value); }
        }
        public IContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { SetProperty(ref _contextMenu, value); }
        }
        public ViewType ViewType
        {
            get { return _viewType; }
            set { SetProperty(ref _viewType, value); }
        }
        public IHierarchicalLibraryItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        public ObservableCollection<object> SelectedItemsInternal
        {
            get { return _selectedItemsInternal; }
            set { SetProperty(ref _selectedItemsInternal, value); }
        }

        public void ClearSelection() {
            SelectedItemsInternal.Clear();
        }

        public bool NavigationVisible
        {
            get { return _navigationVisible; }
            set { SetProperty(ref _navigationVisible, value); }
        }

        void HandleSearch(string searchText, CancellationToken token) {
            var searched = !string.IsNullOrWhiteSpace(searchText);

            lock (_searchLock) {
                HandleSearchItem(searched);
                RefreshSearchItems(searched, token);
            }
        }

        void HandleSearchItem(bool searched) {
            if (SetUp.Items.Contains(SearchItem)) {
                if (searched)
                    SelectedItem = SearchItem;
                else {
                    SetUp.Items.Remove(SearchItem);
                    InitialSelectedItem();
                }
            } else if (searched) {
                SetUp.Items.Add(SearchItem);
                SelectedItem = SearchItem;
            }
        }

        [DoNotObfuscate]
        public void Edit() {
            var item = SelectedItem.FindItem<T>();
            var show = !item.IsEditing;
            item.IsEditing = show;
            if (!show && !item.Items.Any())
                SuggestAddItems();
        }

        protected virtual void SuggestAddItems() {}

        void RefreshSearchItems(bool searched, CancellationToken token) {
            if (searched) {
                SetupSearchItems(token);
                UiHelper.TryOnUiThread(() => SearchItem.ItemsView.TryRefreshIfHasView());
            } else
                SearchItem.Items.Clear();
        }

        protected abstract bool ApplySearchFilter(object obj);

        [DoNotObfuscate]
        public override void DoubleClicked(RoutedEventArgs eventArgs) {
            if (eventArgs.FilterControlFromDoubleClick())
                return;

            var item = eventArgs.FindListBoxItem<T2>();
            if (item != null)
                ActiveItem = item;
            else {
                var item2 = eventArgs.FindListBoxItem<IHierarchicalLibraryItem>();
                if (item2 != null)
                    SelectedItem = item2;
            }
        }

        [DoNotObfuscate]
        public override void DoubleClickedDG(RoutedEventArgs eventArgs) {
            if (eventArgs.FilterControlFromDoubleClick())
                return;

            var item = eventArgs.FindDataGridItem<T2>();
            if (item != null)
                ActiveItem = item;
            else {
                var item2 = eventArgs.FindDataGridItem<IHierarchicalLibraryItem>();
                if (item2 != null)
                    SelectedItem = item2;
            }
        }

        [DoNotObfuscate]
        public virtual void DoubleClickedTV(RoutedEventArgs eventArgs) {
            if (eventArgs.FilterControlFromDoubleClick())
                return;

            var item = eventArgs.FindTreeViewItem<T>();
            if (item == null)
                return;
            var item2 = item as ContentLibraryItemViewModel<Collection>;
            if (item2 == null)
                return;

            ActiveItem = (T2) (object) item2.Model;
        }

        [DoNotObfuscate]
        public virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
            SelectedItem = e.NewValue as IHierarchicalLibraryItem; // TODO
            //IsSelected = true; // Activate the ModSet when Mod selection changes
            e.Handled = true; // We don't want this to buble up to the ModSetViewModel TreeView ;-)
        }

        public abstract void Setup();

        [DoNotObfuscate]
        public void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e) {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem == null)
                return;

            treeViewItem.IsSelected = true;
            e.Handled = true;
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source) {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        protected abstract void InitialSelectedItem();

        protected T FindByModel<T4, T5>(T5 ms)
            where T4 : T, IHaveModel<T5>
            where T5 : class, T2 {
            if (SetUp == null)
                return null;
            var allItems = SetUp.Items.ToArray();

            // TODO: better algorithm?
            return allItems.OfType<T4>().FirstOrDefault(x => x.Model == ms)
                   ??
                   allItems.SelectMany(x => x.Children).OfType<T4>().FirstOrDefault(x => x.Model == ms)
                   ??
                   allItems.SelectMany(x => x.Children)
                       .SelectMany(x => x.Children)
                       .OfType<T4>()
                       .FirstOrDefault(x => x.Model == ms);
            ;
        }

        void SetupSearchItems(CancellationToken token) {
            try {
                SearchItem.UpdateItems(GetSearchItems(token));
            } catch (OperationCanceledException) {}
        }

        IEnumerable<T2> GetSearchItems(CancellationToken token) {
            var libraryGroupViewModels = SetUp.Items.SelectMany(x => x.Children).OfType<T>();
            return SetUp.Items.OfType<T>()
                .Union(libraryGroupViewModels)
                .Where(x => x != SearchItem && x.IsFeatured)
                .SelectMany(x => x.Items)
                .Distinct(Comparer)
                .AsParallel()
                .WithCancellation(token)
                .Where(ApplySearchFilter)
                .OrderByDescending(x => x.SearchScore);
        }
    }
}