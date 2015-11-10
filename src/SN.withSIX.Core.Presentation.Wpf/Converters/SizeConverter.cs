// <copyright company="SIX Networks GmbH" file="SizeConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class SizeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Tools.DefaultSizeReturn;
            double size = (long) value;
            var unit = 0;
            if (parameter != null)
                unit = System.Convert.ToInt32(parameter);

            return Tools.FileUtil.GetFileSize(size, (Tools.FileTools.Units) unit);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}