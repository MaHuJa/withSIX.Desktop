// <copyright company="SIX Networks GmbH" file="PropertyGridExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace SN.withSIX.Core.Presentation.Wpf.Behaviors
{
    public static class PropertyGridExtensions
    {
        public static readonly DependencyProperty CustomizationProperty =
            DependencyProperty.RegisterAttached("Customization", typeof (bool), typeof (PropertyGridExtensions),
                new UIPropertyMetadata(false, CustomizationChangedCallback));

        public static bool GetCustomization(DependencyObject obj) {
            return (bool) obj.GetValue(CustomizationProperty);
        }

        public static void SetCustomization(DependencyObject obj, bool value) {
            obj.SetValue(CustomizationProperty, value);
        }

        static void CustomizationChangedCallback(object sender, DependencyPropertyChangedEventArgs e) {
            var propertyGrid = sender as PropertyGrid;

            if (propertyGrid == null)
                return;
            propertyGrid.Loaded += PropertyGridOnLoaded;
        }

        static void PropertyGridOnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            var propertyGrid = (PropertyGrid) sender;
            var itemsControl =
                ((PropertyItemsControl) propertyGrid.Template.FindName("PART_PropertyItemsControl", propertyGrid));
            // Workaround null for some users...
            // TODO
            if (itemsControl == null)
                return;
            SetupItemsControl(itemsControl);
            propertyGrid.Loaded -= PropertyGridOnLoaded;
        }

        static void SetupItemsControl(PropertyItemsControl itemsControl) {
            itemsControl.TryFindParent<Grid>().Background =
                (SolidColorBrush) Application.Current.FindResource("SixMediumGray");

            itemsControl.GroupStyle.First().ContainerStyle =
                (Style) Application.Current.FindResource("XctkGroupItemStyle");
        }
    }
}