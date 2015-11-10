// <copyright company="SIX Networks GmbH" file="DoubleLessConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class DoubleLessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var d = (double) value;
            var remove = (double) parameter;
            return d - remove;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}