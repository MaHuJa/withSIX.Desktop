// <copyright company="SIX Networks GmbH" file="IconControlConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class IconControlConverter : IValueConverter
    {
        const int DefaultMaxHeight = 14;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var key = value as string;
            if (key == null)
                return null;
            return key.StartsWith("Icon_") ? GetIconControl(parameter, key) : TextBlock(key, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static object TextBlock(string key, object parameter) {
            var maxHeightParameter = parameter as string;
            var tb = new TextBlock {
                Text = key,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily) Application.Current.FindResource("IconFont")
            };
            if (!string.IsNullOrWhiteSpace(maxHeightParameter))
                tb.FontSize = maxHeightParameter.TryDouble();
            return tb;
        }

        static object GetIconControl(object parameter, string key) {
            var obj = Application.Current.FindResource(key) as Canvas;
            return obj == null
                ? null
                : new IconControl {
                    Icon = obj,
                    MaxHeight = ParseMaxHeight(parameter as string)
                };
        }

        static double ParseMaxHeight(string maxHeightParameter) {
            return !string.IsNullOrWhiteSpace(maxHeightParameter) ? maxHeightParameter.TryDouble() : DefaultMaxHeight;
        }
    }
}