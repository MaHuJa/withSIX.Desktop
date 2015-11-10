// <copyright company="SIX Networks GmbH" file="ExecutableToIconImageConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class ExecutableToIconImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var path = value as string;
            return path == null ? null : GetImage(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static BitmapSource GetImage(string filePath) {
            try {
                return TryGetImage(filePath);
            } catch (Exception) {
                return null;
            }
        }

        static BitmapSource TryGetImage(string filePath) {
            using (var sysicon = Icon.ExtractAssociatedIcon(filePath)) {
                // This new call in WPF finally allows us to read/display 32bit Windows file icons!
                return Imaging.CreateBitmapSourceFromHIcon(
                    sysicon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}