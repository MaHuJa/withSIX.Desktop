// <copyright company="SIX Networks GmbH" file="PopupMenuCloseBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls.Primitives;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class PopupMenuCloseBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof (bool),
                typeof (PopupMenuCloseBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(Popup popup) {
            return (bool) popup.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Popup popup, bool value) {
            popup.SetValue(IsEnabledProperty, value);
        }

        static void OnIsEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as Popup;
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
            var popup = (Popup) sender;
            var button = e.OriginalSource as ButtonBase;
            if (button != null && button.ContextMenu == null)
                popup.IsOpen = false;
        }
    }
}