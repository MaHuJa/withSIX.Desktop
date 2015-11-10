// <copyright company="SIX Networks GmbH" file="NotNullToVisibilityConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.SA.Converters
{
    public class NotNullToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var reverse = false;

            if (parameter != null)
                reverse = ((string) parameter).ToLower() == "true";

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
}