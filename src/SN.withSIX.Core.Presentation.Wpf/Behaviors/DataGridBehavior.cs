// <copyright company="SIX Networks GmbH" file="DataGridBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SN.withSIX.Core.Presentation.Wpf.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class DataGridBehavior
    {
        public static readonly DependencyProperty EnableFullRowSelectionProperty =
            DependencyProperty.RegisterAttached(
                "EnableFullRowSelection",
                typeof (bool),
                typeof (DataGridBehavior),
                new UIPropertyMetadata(false, OnEnableFullRowSelectionChanged));
        public static readonly DependencyProperty EnableContextMenuWorkaroundProperty =
            DependencyProperty.RegisterAttached(
                "EnableContextMenuWorkaround",
                typeof (bool),
                typeof (DataGridBehavior),
                new UIPropertyMetadata(false, OnEnableContextMenuWorkaroundChanged));

        public static bool GetEnableFullRowSelection(TreeViewItem treeViewItem) {
            return (bool) treeViewItem.GetValue(EnableFullRowSelectionProperty);
        }

        public static void SetEnableFullRowSelection(
            TreeViewItem treeViewItem, bool value) {
            treeViewItem.SetValue(EnableFullRowSelectionProperty, value);
        }

        static void OnEnableFullRowSelectionChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as DataGrid;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.PreviewMouseDown += SomeGridMouseDown;
            else
                item.PreviewMouseDown -= SomeGridMouseDown;
        }

        public static bool GetEnableContextMenuWorkaround(TreeViewItem treeViewItem) {
            return (bool) treeViewItem.GetValue(EnableContextMenuWorkaroundProperty);
        }

        public static void SetEnableContextMenuWorkaround(
            TreeViewItem treeViewItem, bool value) {
            treeViewItem.SetValue(EnableContextMenuWorkaroundProperty, value);
        }

        static void OnEnableContextMenuWorkaroundChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as DataGrid;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.PreviewMouseRightButtonDown += SomeGridMouseDown2;
            else
                item.PreviewMouseRightButtonDown -= SomeGridMouseDown2;
        }

        static void SomeGridMouseDown(object sender, MouseButtonEventArgs e) {
            var dependencyObject = (DependencyObject) e.OriginalSource;

            //get clicked row from Visual Tree
            while ((dependencyObject != null) && !(dependencyObject is DataGridRow))
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject.FindVisualTreeRoot());

            var row = dependencyObject as DataGridRow;
            if (row == null)
                return;

            row.IsSelected = true;
        }

        static void SomeGridMouseDown2(object sender, MouseButtonEventArgs e) {
            var dg = (DataGrid) sender;
            var dependencyObject = (DependencyObject) e.OriginalSource;
            var row = dependencyObject.FindItem<DataGridRow>();
            if (row != null && !row.IsSelected)
                dg.SelectedItem = row.Item; // row.IsSelected = true;
        }
    }
}