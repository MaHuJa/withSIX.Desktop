// <copyright company="SIX Networks GmbH" file="PingToFillConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class PingToFillConverter : IValueConverter
    {
        static readonly string defaultReturn = SixColors.SixSoftGray;
        static readonly string defaultReturnBad = SixColors.SixSoftRed;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var par = System.Convert.ToInt32((string) parameter);
            if (value == null) {
                if (par == 0)
                    return defaultReturnBad;
                return defaultReturn;
            }

            return GetBarColor((long) value, par);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        string GetBarColor(long val, int par) {
            /*
    3 bars green  =   1 -  25 (delta 25)
    2 bars green  =  26 -  75 (delta 50)
    2 bars orange =  76 - 175 (delta 100)
    1 bars orange = 176 - 375 (delta 200)
    1 bars red    = 376+
    no bars           0 and 9999 (no response)
             */

            int amount;
            if (val == Common.MagicPingValue || val == 0)
                return defaultReturn;

            if (val > 175) {
                amount = 1;
                if (par > amount)
                    return defaultReturn;
                if (val > 375)
                    return SixColors.SixSoftRed;
                return SixColors.SixOrange;
            }

            if (val > 25) {
                amount = 2;
                if (par > amount)
                    return defaultReturn;
                if (val > 75)
                    return SixColors.SixOrange;
                return SixColors.SixGreen;
            }

            amount = 3;
            return par > amount ? defaultReturn : SixColors.SixGreen;
        }

        #endregion
    }
}