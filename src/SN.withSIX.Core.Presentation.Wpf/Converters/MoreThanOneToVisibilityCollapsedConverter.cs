// <copyright company="SIX Networks GmbH" file="MoreThanOneToVisibilityCollapsedConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class MoreThanOneToVisibilityCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var inverse = parameter != null && ((string) parameter).TryBool();
            if ((int) value > 1)
                return inverse ? Visibility.Collapsed : Visibility.Visible;

            return inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}