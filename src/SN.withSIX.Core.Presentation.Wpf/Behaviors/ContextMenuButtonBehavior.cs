// <copyright company="SIX Networks GmbH" file="ContextMenuButtonBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SN.withSIX.Core.Presentation.Wpf.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class ContextMenuButtonBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof (bool),
                typeof (ContextMenuButtonBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(Window window) {
            return (bool) window.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Window window, bool value) {
            window.SetValue(IsEnabledProperty, value);
        }

        static void OnIsEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as Button;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.Click += OnClick;
            else
                item.Click -= OnClick;
        }

        static void OnClick(object sender, RoutedEventArgs e) {
            var button = (Button) sender;
            var cm = button.FindContextMenu();
            cm.IsEnabled = true;
            cm.PlacementTarget = button;
            cm.Placement = PlacementMode.Bottom;
            cm.IsOpen = true;
        }
    }
}