// <copyright company="SIX Networks GmbH" file="UniformGridBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class UniformGridBehavior
    {
        #region ItemWidth

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.RegisterAttached(
                "ItemWidth",
                typeof (double),
                typeof (UniformGridBehavior),
                new UIPropertyMetadata(0.0, OnItemWidthChanged));

        public static double GetItemWidth(UniformGrid grid) {
            return (double) grid.GetValue(ItemWidthProperty);
        }

        public static void SetItemWidth(UniformGrid grid, double value) {
            grid.SetValue(ItemWidthProperty, value);
        }

        static void OnItemWidthChanged(
            DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var grid = depObj as UniformGrid;
            if (grid == null)
                return;

            if (e.NewValue is double == false)
                return;

            var val = (double) e.NewValue;
            grid.WhenControlActivated(d => {
                d(grid.WhenAnyValue(x => x.ActualWidth)
                    .Select(x => GetColumns(x, val))
                    .Select(x => x < 1 ? 1 : x)
                    .BindTo(grid, x => x.Columns));
            });
        }

        static int GetColumns(double x, double val) {
            return (int) (x/val);
        }

        #endregion
    }
}