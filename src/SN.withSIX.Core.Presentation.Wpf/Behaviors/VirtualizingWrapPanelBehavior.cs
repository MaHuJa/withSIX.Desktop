// <copyright company="SIX Networks GmbH" file="VirtualizingWrapPanelBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using Telerik.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    // TODO: Convert this actually to behavior directly built into the virtualizing wrap panel?
    public static class VirtualizingWrapPanelBehavior
    {
        #region ItemWidth

        public static readonly DependencyProperty MinItemWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinItemWidth",
                typeof (double),
                typeof (VirtualizingWrapPanelBehavior),
                new UIPropertyMetadata(0.0, OnMinItemWidthChanged));

        public static double GetMinItemWidth(VirtualizingWrapPanel grid) {
            return (double) grid.GetValue(MinItemWidthProperty);
        }

        public static void SetMinItemWidth(VirtualizingWrapPanel grid, double value) {
            grid.SetValue(MinItemWidthProperty, value);
        }

        static void OnMinItemWidthChanged(
            DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var panel = depObj as VirtualizingWrapPanel;
            if (panel == null)
                return;

            if (e.NewValue is double == false)
                return;

            var val = (double) e.NewValue;
            panel.WhenControlActivated(d => {
                var control = ItemsControl.GetItemsOwner(panel);
                d(Tuple.Create(panel, control).WhenAnyValue(x => x.Item1.ActualWidth, x => x.Item2.Items.Count)
                    .Throttle(TimeSpan.FromMilliseconds(250), RxApp.MainThreadScheduler)
                    .Select(x => GetOptimalWidth(x.Item1, val, x.Item2))
                    .BindTo(panel, x => x.ItemWidth));
            });
        }

        static double GetOptimalWidth(double x, double val, int itemsCount) {
            var optimalWidth = (int) (x/GetOptimalColumns(x, val, itemsCount));
            return optimalWidth > val ? optimalWidth : val;
        }

        static int GetOptimalColumns(double x, double val, int itemsCount) {
            var columns = (int) (x/val);
            if (columns > itemsCount)
                columns = itemsCount;
            return columns > 0 ? columns : 1;
        }

        #endregion
    }
}