// <copyright company="SIX Networks GmbH" file="ProgressStateConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class ProgressStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int b;

            var bl = value as int?;
            if (bl.HasValue)
                b = bl.Value;
            else
                return TaskbarItemProgressState.None;
            switch (b) {
            case 0:
                return TaskbarItemProgressState.None;
            case 1:
                return TaskbarItemProgressState.Normal;
            case 2:
                return TaskbarItemProgressState.Paused;
            case 3:
                return TaskbarItemProgressState.Indeterminate;
            case 4:
                return TaskbarItemProgressState.Error;
            default:
                return TaskbarItemProgressState.None;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}