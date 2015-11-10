// <copyright company="SIX Networks GmbH" file="HasNotesToForegroundConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core.Applications;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class HasNotesToForegroundConverter : IValueConverter
    {
        static readonly string _true = SixColors.SixBlue;
        static readonly string _false = SixColors.SixGray;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool) value)
                return _true;
            return _false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}