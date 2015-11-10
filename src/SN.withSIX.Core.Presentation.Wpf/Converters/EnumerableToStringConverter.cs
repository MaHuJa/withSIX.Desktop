// <copyright company="SIX Networks GmbH" file="EnumerableToStringConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class EnumerableToStringConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;
        static readonly string defaultConcat = ", ";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var collection = value as IEnumerable<String>;
            return collection == null ? DefaultReturn : String.Join(defaultConcat, collection);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}