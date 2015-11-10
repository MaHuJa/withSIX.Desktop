// <copyright company="SIX Networks GmbH" file="EntitiesToStringConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class EntitiesToStringConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;
        static readonly string Concat = ", ";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DefaultReturn;

            var collection = (IEnumerable) value;
            var c = collection.Cast<IContact>().Select(entity => entity.DisplayName).ToList();

            return String.Join(Concat, c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}