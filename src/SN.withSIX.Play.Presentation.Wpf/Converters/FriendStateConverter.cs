// <copyright company="SIX Networks GmbH" file="FriendStateConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class FriendStateConverter : IValueConverter
    {
        static readonly string DefaultReturn = null;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var r = DefaultReturn;

            if (value == null)
                return r;
            var size = (int) value;

            switch (size) {
            case 0:
                r = "Red";
                break;
            case 1:
                r = "Green";
                break;
            case 9:
                r = "Orange";
                break;
            }

            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}