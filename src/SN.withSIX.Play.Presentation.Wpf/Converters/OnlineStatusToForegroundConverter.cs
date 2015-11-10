// <copyright company="SIX Networks GmbH" file="OnlineStatusToForegroundConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Core.Applications;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class OnlineStatusToForegroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || !(value is OnlineStatus))
                return SixColors.OnlineStatusOffline;

            switch ((OnlineStatus) value) {
            case OnlineStatus.Offline:
                return SixColors.OnlineStatusOffline;
            case OnlineStatus.Online:
                return SixColors.OnlineStatusOnline;
            //case OnlineStatus.Busy:
            //    return SixColors.OnlineStatusBusy;
            case OnlineStatus.Away:
                return SixColors.OnlineStatusAway;
            default:
                return SixColors.OnlineStatusOffline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}