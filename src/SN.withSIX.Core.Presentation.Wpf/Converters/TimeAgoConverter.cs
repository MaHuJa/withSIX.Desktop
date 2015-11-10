// <copyright company="SIX Networks GmbH" file="TimeAgoConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class TimeAgoConverter : IValueConverter
    {
        const string DefaultReturn = "Never";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? DefaultReturn : ((DateTime) value).Ago();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}