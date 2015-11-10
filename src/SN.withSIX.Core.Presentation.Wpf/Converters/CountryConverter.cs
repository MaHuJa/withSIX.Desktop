// <copyright company="SIX Networks GmbH" file="CountryConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class CountryConverter : IValueConverter
    {
        static readonly string DefaultReturn = "Empty";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DefaultReturn;
            var str = (string) value;
            if (String.IsNullOrWhiteSpace(str))
                return DefaultReturn;

            string result;
            if (!CountryFlagsMapping.CountryDict.TryGetValue((string) value, out result))
                return DefaultReturn;

            if (Enum.IsDefined(typeof (CountryFlags), result))
                return result;

            return DefaultReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}