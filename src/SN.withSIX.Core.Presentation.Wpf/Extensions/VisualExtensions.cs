// <copyright company="SIX Networks GmbH" file="VisualExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public static class VisualExtensions
    {
        public static Action<Task> Waiter = WaitWithPumping;

        public static void WaitSpecial(this Task task) {
            Waiter(task);
        }

        public static void WaitWithPumping(this Task task) {
            if (task == null)
                throw new ArgumentNullException("task");
            var nestedFrame = new DispatcherFrame();
            task.ContinueWith(_ => nestedFrame.Continue = false);
            Dispatcher.PushFrame(nestedFrame);
            task.WaitAndUnwrapException();
        }

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