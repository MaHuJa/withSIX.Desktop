// <copyright company="SIX Networks GmbH" file="SpeedConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class SpeedConverter : IValueConverter
    {
        static readonly string defaultReturn = String.Empty;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return defaultReturn;

            var unit = 0;
            if (parameter != null)
                unit = System.Convert.ToInt32(parameter);

            var l = (long) value;
            return ConvertSpeed(l, unit);
        }

        public static object ConvertSpeed(long l, int unit = 0) {
            double size = l;
            return size.FormatSpeed((Tools.FileTools.Units) unit);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}