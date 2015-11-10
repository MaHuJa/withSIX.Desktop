// <copyright company="SIX Networks GmbH" file="GroupItemBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using Telerik.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    // TODO: Convert this actually to behavior directly built into the virtualizing wrap panel?
    public static class GroupItemBehavior
    {
        // TODO: Configurable RowHeight
        const int RowHeight = 72;

        #region ItemWidth

        public static readonly DependencyProperty MinColumnWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinColumnWidth",
                typeof (double),
                typeof (GroupItemBehavior),
                new UIPropertyMetadata(0.0, OnMinColumnWidthChanged));

        public static double GetMinColumnWidth(VirtualizingWrapPanel grid) {
            return (double) grid.GetValue(MinColumnWidthProperty);
        }

        public static void SetMinColumnWidth(VirtualizingWrapPanel grid, double value) {
            grid.SetValue(MinColumnWidthProperty, value);
        }

        static void OnMinColumnWidthChanged(
            DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var groupItem = depObj as GroupItem;
            if (groupItem == null)
                return;

            if (e.NewValue is double == false)
                return;

            // NB: This would double up when we change the MinWidth...
            groupItem.WhenControlActivated(d => {
                var expander = (Expander) groupItem.Template.FindName("Expander", groupItem);
                var content = (ItemsPresenter) expander.Content;
                IDisposable obs2 = null;
                d(groupItem.WhenAnyValue(x => x.DataContext)
                    .OfType<CollectionViewGroup>()
                    .Subscribe(
                        x => {
                            if (obs2 != null) {
                                obs2.Dispose();
                                obs2 = null;
                            }
                            if (x == null)
                                return;
                            d(obs2 = Tuple.Create(x, groupItem)
                                .WhenAnyValue(y => y.Item1.Items.Count, y => y.Item2.ActualWidth)
                                .Throttle(TimeSpan.FromMilliseconds(250), RxApp.MainThreadScheduler)
                                .Select(y => GetDesiredHeight(y.Item1, y.Item2, (double) e.NewValue))
                                .BindTo(content, y => y.Height));
                        }));
            });
        }

        class State
        {
            public double MinWidth { get; set; }
            public RoutedEventHandler Delegate { get; set; }
        }

        static int GetDesiredHeight(int totalItems, double availableWidth, double minWidth) {
            return GetRows(totalItems, availableWidth, minWidth)*RowHeight;
        }

        static int GetRows(int totalItems, double availableWidth, double minWidth) {
            var rows =
                (int)
                    Math.Round((double) totalItems/GetOptimalColumns(availableWidth, minWidth, totalItems),
                        MidpointRounding.AwayFromZero);
            return rows > 0 ? rows : 1;
        }

        static int GetOptimalColumns(double availableWidth, double columnWidth, int itemsCount) {
            var columns = (int) (availableWidth/columnWidth);
            if (columns > itemsCount)
                columns = itemsCount;
            return columns > 0 ? columns : 1;
        }

        #endregion
    }
}