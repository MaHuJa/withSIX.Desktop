// <copyright company="SIX Networks GmbH" file="TextBoxBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public class TextBoxBehavior
    {
        public static readonly DependencyProperty SelectAllTextOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllTextOnFocus",
                typeof (bool),
                typeof (TextBoxBehavior),
                new UIPropertyMetadata(false, OnSelectAllTextOnFocusChanged));

        public static bool GetSelectAllTextOnFocus(TextBox textBox) {
            return (bool) textBox.GetValue(SelectAllTextOnFocusProperty);
        }

        public static void SetSelectAllTextOnFocus(TextBox textBox, bool value) {
            textBox.SetValue(SelectAllTextOnFocusProperty, value);
        }

        static void OnSelectAllTextOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var textBox = d as TextBox;
            if (textBox == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue) {
                textBox.GotFocus += SelectAll;
                textBox.PreviewMouseDown += IgnoreMouseButton;
            } else {
                textBox.GotFocus -= SelectAll;
                textBox.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        static void SelectAll(object sender, RoutedEventArgs e) {
            var textBox = e.OriginalSource as TextBox;
            if (textBox == null)
                return;
            textBox.SelectAll();
        }

        static void IgnoreMouseButton(object sender, MouseButtonEventArgs e) {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.IsKeyboardFocusWithin)
                return;

            e.Handled = true;
            textBox.Focus();
        }
    }
}