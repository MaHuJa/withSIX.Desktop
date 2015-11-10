// <copyright company="SIX Networks GmbH" file="FilteredConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class FilteredConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var r = DefaultReturn;
            if (value == null)
                return r;
            if ((bool) value)
                r = " (filtered)";

            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}