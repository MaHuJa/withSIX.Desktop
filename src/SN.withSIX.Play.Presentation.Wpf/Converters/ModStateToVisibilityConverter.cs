// <copyright company="SIX Networks GmbH" file="ModStateToVisibilityConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SN.withSIX.Core.Applications;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class ModStateToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Collapsed;

            var state = (ContentState) value;
            if (state == ContentState.Uptodate || state == ContentState.Local)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class LocalModVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Collapsed;

            var state = (ContentState) value;
            if (state == ContentState.Local)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ModStateToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return SixColors.SixSoftGray;

            return TryGetContentColor(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static string TryGetContentColor(object value) {
            try {
                return GetContentColor(value);
            } catch (Exception) {
                return SixColors.SixSoftGray;
            }
        }

        static string GetContentColor(object value) {
            var state = (ContentState) value;
            if (state == ContentState.Uptodate || state == ContentState.Local)
                return SixColors.SixGreen;

            if (state == ContentState.Incompatible)
                return SixColors.SixSoftRed;

            if (state == ContentState.Unverified)
                return SixColors.SixYellow;

            return state == ContentState.NotInstalled ? SixColors.SixGray : SixColors.SixOrange;
        }

        #endregion
    }
}