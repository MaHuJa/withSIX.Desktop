// <copyright company="SIX Networks GmbH" file="MarginSetter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public class MarginSetter
    {
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof (Thickness), typeof (MarginSetter),
                new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        public static Thickness GetMargin(DependencyObject obj) {
            return (Thickness) obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value) {
            obj.SetValue(MarginProperty, value);
        }

        public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e) {
            var panel = sender as Panel;

            if (panel == null)
                return;
            panel.Loaded += panel_Loaded;
        }

        static void panel_Loaded(object sender, RoutedEventArgs e) {
            var panel = (Panel) sender;

            foreach (var child in panel.Children) {
                var fe = child as FrameworkElement;

                if (fe == null)
                    continue;

                fe.Margin = GetMargin(panel);
            }
        }
    }
}