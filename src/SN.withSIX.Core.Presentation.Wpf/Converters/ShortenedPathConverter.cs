// <copyright company="SIX Networks GmbH" file="ShortenedPathConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class ShortenedPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var str = (string) value;
            if (str == null)
                return null;
            if (str.Length <= 160)
                return str;
            var split = str.Split('\\', '/');
            return split.Length > 2 ? String.Join("\\", split.Take(2).Concat(new[] {"...", split.Last()})) : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}