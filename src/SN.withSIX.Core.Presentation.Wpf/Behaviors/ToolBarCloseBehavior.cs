// <copyright company="SIX Networks GmbH" file="ToolBarCloseBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class ToolBarCloseBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof (bool),
                typeof (ToolBarCloseBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(ToolBar popup) {
            return (bool) popup.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ToolBar popup, bool value) {
            popup.SetValue(IsEnabledProperty, value);
        }

        static void OnIsEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as ToolBar;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.PreviewMouseLeftButtonUp += OnClick;
            else
                item.PreviewMouseLeftButtonUp -= OnClick;
        }

        static void OnClick(object sender, RoutedEventArgs e) {
            var popup = (ToolBar) sender;
            var button = e.OriginalSource as ButtonBase;

            // Closes too soon??
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                if (button != null && button.ContextMenu == null)
                    popup.IsOverflowOpen = false;
            }));
        }
    }
}