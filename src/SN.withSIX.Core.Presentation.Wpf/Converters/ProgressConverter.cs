// <copyright company="SIX Networks GmbH" file="ProgressConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class ProgressConverter : IValueConverter
    {
        const double DefaultReturn = 0.0;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? DefaultReturn : TryParseValue(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static object TryParseValue(object value) {
            try {
                return ((double) value/100.0);
            } catch {
                try {
                    return ((int) value/100.0);
                } catch {}
            }

            return DefaultReturn;
        }

        #endregion
    }
}