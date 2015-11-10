// <copyright company="SIX Networks GmbH" file="BooleanToVisibilityHiddenConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class BooleanToVisibilityHiddenConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var inverse = parameter != null && ((string) parameter).TryBool();
            var val = inverse
                ? !(bool) value
                : (bool) value;

            if (val)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}