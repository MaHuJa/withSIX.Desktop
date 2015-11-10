// <copyright company="SIX Networks GmbH" file="ActionStatusToColorConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core.Applications;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class ActionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var val = (ActionStatus) value;
            switch (val) {
            case ActionStatus.NoGameFound:
                return SixColors.SixSoftRed;
            case ActionStatus.Play:
                return SixColors.SixGreen;
            case ActionStatus.Update:
                return SixColors.SixOrange;
            case ActionStatus.Diagnose:
                return SixColors.SixYellow;
            default: {
                return SixColors.SixGray;
            }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}