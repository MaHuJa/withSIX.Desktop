// <copyright company="SIX Networks GmbH" file="InverseBooleanConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    [ValueConversion(typeof (bool), typeof (bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}