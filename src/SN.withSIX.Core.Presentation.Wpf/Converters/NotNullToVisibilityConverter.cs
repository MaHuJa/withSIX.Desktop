// <copyright company="SIX Networks GmbH" file="NotNullToVisibilityConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class NotNullToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var reverse = false;

            if (parameter != null)
                reverse = ((string) parameter).TryBool();

            if (reverse) {
                return value != null
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            return value != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NotNullOrEmptyToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var reverse = false;

            if (parameter != null)
                reverse = ((string) parameter).TryBool();

            if (reverse) {
                return !string.IsNullOrWhiteSpace((string) value)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            return !string.IsNullOrWhiteSpace((string) value)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}