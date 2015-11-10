// <copyright company="SIX Networks GmbH" file="DialogExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Caliburn.Micro;
using MoreLinq;
using SN.withSIX.Core.Extensions;
using Action = System.Action;

namespace SN.withSIX.Play.Applications.Extensions
{
    [Obsolete("Thesea re mostly helpers for Wpf apps and have no place in VM/apps")]
    public static class DialogExtensions
    {
        public static void RefreshView(this CollectionViewSource @this) {
            @this.View.Refresh();
        }

        public static bool RefreshIfHasView(this CollectionViewSource @this) {
            if (@this == null || @this.View == null)
                return false;
            @this.RefreshView();
            return true;
        }

        public static bool RefreshIfHasView(this ICollectionView @this) {
            if (@this == null)
                return false;
            @this.Refresh();
            return true;
        }

        public static bool TryRefreshIfHasView(this CollectionViewSource @this) {
            if (@this == null || @this.View == null)
                return false;

            try {
                @this.RefreshView();
                return true;
            } catch (InvalidOperationException) {
                return false;
            } catch (ArgumentException) {
                return false;
            }
        }

        public static bool TryRefreshIfHasView(this ICollectionView @this) {
            if (@this == null)
                return false;

            try {
                @this.Refresh();
                return true;
            } catch (InvalidOperationException) {
                return false;
            } catch (ArgumentException) {
                return false;
            }
        }

        public static void RefreshViewItem(this CollectionViewSource vs, object sender = null) {
            Contract.Requires<ArgumentNullException>(vs != null);
            RefreshViewItem(vs.View, sender);
        }

        public static void RefreshViewItem(this ICollectionView view, object sender = null) {
            Contract.Requires<ArgumentNullException>(view != null);

            var current = view.CurrentItem;
            var editableCollectionView = view as IEditableCollectionView;
            if (editableCollectionView != null && sender != null) {
                editableCollectionView.EditItem(sender);
                editableCollectionView.CommitEdit();
            } else
                view.Refresh();

            //if (sender == current) view.MoveCurrentTo(null);

            view.MoveCurrentTo(current);
            //Ensure that the previously current item is maintained after the refresh operation   
        }

        public static bool TryRefreshViewItem(this CollectionViewSource vs, object sender = null) {
            return vs != null && TryRefreshViewItem(vs.View);
        }

        public static bool TryRefreshViewItem(this ICollectionView view, object sender = null) {
            if (view == null)
                return false;

            try {
                RefreshViewItem(view, sender);
                return true;
            } catch (InvalidOperationException) {
                return false;
            } catch (ArgumentException) {
                return false;
            }
        }

        public static void RefreshViewItems(this CollectionViewSource vs, object[] senders = null) {
            Contract.Requires<ArgumentNullException>(vs != null);
            RefreshViewItems(vs.View, senders);
        }

        public static ICollectionView GetDefaultView(this IEnumerable source) {
            return CollectionViewSource.GetDefaultView(source);
        }

        public static void RefreshViewItems(this ICollectionView view, object[] senders = null) {
            Contract.Requires<ArgumentNullException>(view != null);

            var current = view.CurrentItem;
            var editableCollectionView = view as IEditableCollectionView;
            if (editableCollectionView != null && senders != null && senders.Any()) {
                senders.ForEach(sender => {
                    editableCollectionView.EditItem(sender);
                    editableCollectionView.CommitEdit();
                });
            } else
                view.Refresh();

            //if (sender == current) view.MoveCurrentTo(null);

            view.MoveCurrentTo(current);
            //Ensure that the previously current item is maintained after the refresh operation
        }

        public static void ActiveLiveSorting(this CollectionViewSource collectionView,
            IEnumerable<string> involvedProperties) {
            ActiveLiveSorting(collectionView.View, involvedProperties);
        }

        public static void ActiveLiveSorting(this ICollectionView collectionView, IEnumerable<string> involvedProperties) {
            var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
            if (collectionViewLiveShaping == null)
                return;
            if (!collectionViewLiveShaping.CanChangeLiveSorting)
                return;
            collectionViewLiveShaping.LiveSortingProperties.Clear();
            foreach (var propName in involvedProperties)
                collectionViewLiveShaping.LiveSortingProperties.Add(propName);
            collectionViewLiveShaping.IsLiveSorting = true;
        }

        public static void ActiveLiveGrouping(this CollectionViewSource collectionView,
            IEnumerable<string> involvedProperties) {
            ActiveLiveGrouping(collectionView.View, involvedProperties);
        }

        public static void ActiveLiveGrouping(this ICollectionView collectionView,
            IEnumerable<string> involvedProperties) {
            var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
            if (collectionViewLiveShaping == null)
                return;
            if (!collectionViewLiveShaping.CanChangeLiveGrouping)
                return;
            collectionViewLiveShaping.LiveGroupingProperties.Clear();
            foreach (var propName in involvedProperties)
                collectionViewLiveShaping.LiveGroupingProperties.Add(propName);
            collectionViewLiveShaping.IsLiveGrouping = true;
        }

        public static void ActiveLiveFiltering(this CollectionViewSource collectionView,
            IEnumerable<string> involvedProperties) {
            ActiveLiveFiltering(collectionView.View, involvedProperties);
        }

        public static void ActiveLiveFiltering(this ICollectionView collectionView,
            IEnumerable<string> involvedProperties) {
            var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
            if (collectionViewLiveShaping == null)
                return;
            if (!collectionViewLiveShaping.CanChangeLiveFiltering)
                return;
            collectionViewLiveShaping.LiveFilteringProperties.Clear();
            foreach (var propName in involvedProperties)
                collectionViewLiveShaping.LiveFilteringProperties.Add(propName);
            collectionViewLiveShaping.IsLiveFiltering = true;
        }

        public static void UpdateView(this ICollectionView @this, Action action) {
            var disposable = @this.DeferRefresh();
            action();
            disposable.Dispose();
        }

        public static T[] ToArray<T>(this CollectionViewSource viewSource) {
            Contract.Requires<ArgumentNullException>(viewSource != null);

            T[] list = null;
            viewSource.Dispatcher.Invoke(() => {
                var view = viewSource.View;
                if (view != null)
                    list = view.Cast<T>().ToArray();
            });
            return list;
        }

        public static T[] ToArray<T>(this ICollectionView view) {
            Contract.Requires<ArgumentNullException>(view != null);

            T[] list = null;
            Execute.OnUIThread(() => { list = view.Cast<T>().ToArray(); });

            return list;
        }

        public static CollectionViewSource CreateCollectionViewSource(this IEnumerable source,
            IList<SortDescription> sortDescriptions, IList<PropertyGroupDescription> groupDescriptions,
            IEnumerable<string> filterDescriptions, Predicate<object> filter, bool live = false) {
            ConfirmOnDispatcherThread();
            var vs = new CollectionViewSource {Source = source};
            Setup(vs.View, sortDescriptions, groupDescriptions, filterDescriptions, filter, live);
            return vs;
        }

        public static void Setup(this ICollectionView vs, IList<SortDescription> sortDescriptions = null,
            IList<PropertyGroupDescription> groupDescriptions = null, IEnumerable<string> filterDescriptions = null,
            Predicate<object> filter = null, bool live = false) {
            var d = vs.DeferRefresh();
            vs.SortDescriptions.Clear();
            if (sortDescriptions != null)
                vs.SortDescriptions.AddRange(sortDescriptions);
            vs.GroupDescriptions.Clear();
            if (groupDescriptions != null)
                vs.GroupDescriptions.AddRange(groupDescriptions);

            vs.Filter = filter;

            if (live)
                EnableLiveShaping(vs, sortDescriptions, groupDescriptions, filterDescriptions);
            d.Dispose();
        }

        public static void EnableLiveShaping(this ICollectionView vs,
            IEnumerable<SortDescription> sortDescriptions = null,
            IEnumerable<PropertyGroupDescription> groupDescriptions = null,
            IEnumerable<string> filterDescriptions = null) {
            var d = vs.DeferRefresh();
            // TODO: These are not realtime updated when the descriptions on the view change!
            if (filterDescriptions != null)
                vs.ActiveLiveFiltering(filterDescriptions);
            if (sortDescriptions != null)
                vs.ActiveLiveSorting(sortDescriptions.Select(x => x.PropertyName));
            if (groupDescriptions != null)
                vs.ActiveLiveGrouping(groupDescriptions.Select(x => x.PropertyName));
            d.Dispose();
        }

        public static void DisableLiveShaping(this ICollectionView vs) {
            var collectionViewLiveShaping = vs as ICollectionViewLiveShaping;
            if (collectionViewLiveShaping == null)
                return;
            var d = vs.DeferRefresh();
            collectionViewLiveShaping.IsLiveFiltering = false;
            collectionViewLiveShaping.IsLiveSorting = false;
            collectionViewLiveShaping.IsLiveGrouping = false;
            d.Dispose();
        }

        public static ICollectionView CreateCollectionView(this IList source,
            IList<SortDescription> sortDescriptions = null, IList<PropertyGroupDescription> groupDescriptions = null,
            IEnumerable<string> filterDescriptions = null, Predicate<object> filter = null, bool live = false) {
            ConfirmOnDispatcherThread();
            var vs = new ListCollectionView(source);
            Setup(vs, sortDescriptions, groupDescriptions, filterDescriptions, filter, live);

            return vs;
        }

        public static ICollectionView SetupDefaultCollectionView(this IEnumerable source,
            IList<SortDescription> sortDescriptions = null, IList<PropertyGroupDescription> groupDescriptions = null,
            IEnumerable<string> filterDescriptions = null, Predicate<object> filter = null, bool live = false) {
            ConfirmOnDispatcherThread();
            var vs = CollectionViewSource.GetDefaultView(source);
            Setup(vs, sortDescriptions, groupDescriptions, filterDescriptions, filter, live);

            return vs;
        }

        public static void EnableCollectionSynchronization(this IEnumerable source, Object lockObject) {
            ConfirmOnDispatcherThread();
            BindingOperations.EnableCollectionSynchronization(source, lockObject);
        }

        public static void DisableCollectionSynchronization(this IEnumerable source) {
            BindingOperations.DisableCollectionSynchronization(source);
        }

        static void ConfirmOnDispatcherThread() {
            var app = Application.Current;
            if (app == null)
                throw new InvalidOperationException("No application running");
            var dispatcher = app.Dispatcher;
            if (dispatcher == null)
                throw new InvalidOperationException("No dispatcher available");
            if (!dispatcher.CheckAccess())
                throw new InvalidOperationException("Not dispatcher thread");
        }
    }

    [Obsolete("These are mostly helpers for Wpf apps and have no place in VM/apps")]
    public static class VisualExtensions
    {
        public static T GetDescendantByType<T>(this Visual element) where T : class {
            if (element == null)
                return default(T);
            if (element is T)
                return element as T;
            T foundElement = null;
            var el = element as FrameworkElement;
            if (el != null)
                el.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType<T>(visual);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        public static T GetDescendantByType<T>(this DependencyObject element) where T : class {
            if (element == null)
                return default(T);
            if (element is T)
                return element as T;
            T foundElement = null;
            var el = element as FrameworkElement;
            if (el != null)
                el.ApplyTemplate();

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType<T>(visual);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        public static T FindDataGridItem<T>(this DependencyObject dep) where T : class {
            Contract.Requires<ArgumentNullException>(dep != null);

            var row = FindItem<DataGridRow>(dep);
            if (row == null)
                return default(T);
            return row.Item as T;
        }

        public static T FindListBoxItem<T>(this DependencyObject dep) where T : class {
            Contract.Requires<ArgumentNullException>(dep != null);

            var row = FindItem<ListBoxItem>(dep);

            if (row != null)
                return row.DataContext as T;

            return default(T);
        }

        public static DependencyObject FindVisualTreeRoot(this DependencyObject d) {
            var current = d;
            var result = d;

            while (current != null) {
                result = current;
                if (current is Visual || current is Visual3D)
                    break;
                // If we're in Logical Land then we must walk 
                // up the logical tree until we find a 
                // Visual/Visual3D to get us back to Visual Land.
                current = LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

        public static T FindItem<T>(this DependencyObject dep) where T : class {
            Contract.Requires<ArgumentNullException>(dep != null);

            while ((dep != null) && !(dep is T))
                dep = VisualTreeHelper.GetParent(FindVisualTreeRoot(dep));

            return dep as T;
        }

        public static ContextMenu FindContextMenu(this DependencyObject dep) {
            Contract.Requires<ArgumentNullException>(dep != null);

            ContextMenu cm = null;
            var fe = dep as FrameworkElement;
            if (fe != null && fe.ContextMenu != null)
                cm = fe.ContextMenu;

            while ((dep != null) && (cm == null)) {
                dep = VisualTreeHelper.GetParent(FindVisualTreeRoot(dep));
                fe = dep as FrameworkElement;
                if (fe != null && fe.ContextMenu != null)
                    cm = fe.ContextMenu;
            }

            return cm;
        }

        public static T FindItem<T>(this MouseEventArgs args) where T : class {
            Contract.Requires<ArgumentNullException>(args != null);

            var dep = (DependencyObject) args.OriginalSource;
            return dep == null ? default(T) : dep.FindItem<T>();
        }

        public static ContextMenu FindContextMenu(this MouseEventArgs args) {
            Contract.Requires<ArgumentNullException>(args != null);

            var dep = (DependencyObject) args.OriginalSource;
            return dep == null ? null : dep.FindContextMenu();
        }

        public static T FindTreeViewItem<T>(this DependencyObject dep) where T : class {
            Contract.Requires<ArgumentNullException>(dep != null);

            var row = dep.FindItem<TreeViewItem>();
            if (row != null)
                return row.DataContext as T;

            return default(T);
        }

        public static T FindTreeViewItem<T>(this RoutedEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);
            return FindTreeViewItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static T FindListBoxItem<T>(this MouseButtonEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);

            return FindListBoxItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static T FindTreeViewItem<T>(this MouseButtonEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);

            return FindTreeViewItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static T FindDataGridItem<T>(this MouseButtonEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);

            return FindDataGridItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static T FindListBoxItem<T>(this RoutedEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);

            return FindListBoxItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static T FindDataGridItem<T>(this RoutedEventArgs Args) where T : class {
            Contract.Requires<ArgumentNullException>(Args != null);

            return FindDataGridItem<T>((DependencyObject) Args.OriginalSource);
        }

        public static bool FilterControlFromDoubleClick(this RoutedEventArgs args) {
            var dep = (DependencyObject) args.OriginalSource;

            return dep.FindItem<ButtonBase>() != null;
        }
    }
}