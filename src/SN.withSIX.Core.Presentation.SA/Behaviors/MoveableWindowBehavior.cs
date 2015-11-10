// <copyright company="SIX Networks GmbH" file="MoveableWindowBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.SA.Behaviors
{
    public static class MoveableWindowBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof (bool),
                typeof (MoveableWindowBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(Window window) {
            return (bool) window.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Window window, bool value) {
            window.SetValue(IsEnabledProperty, value);
        }

        static void OnIsEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var item = depObj as Window;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.MouseLeftButtonDown += OnMouseLeftButtonDown;
            else
                item.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var window = (Window) sender;
            var pos = e.GetPosition(window);
            if (pos.Y < 28)
                window.DragMove();
        }
    }
}