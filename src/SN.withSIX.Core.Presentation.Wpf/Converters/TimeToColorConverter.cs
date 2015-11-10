// <copyright company="SIX Networks GmbH" file="TimeToColorConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class TimeToColorConverter : IValueConverter
    {
        public static SolidColorBrush Night = new SolidColorBrush(Color.FromArgb(255, 171, 171, 171));
        public static SolidColorBrush Day = new SolidColorBrush(Colors.Yellow);

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var dateTime = value as DateTime?;
            if (value == null)
                return null;

            if (dateTime.Value.Hour < 5 || dateTime.Value.Hour > 19)
                return Night;

            return Day;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}