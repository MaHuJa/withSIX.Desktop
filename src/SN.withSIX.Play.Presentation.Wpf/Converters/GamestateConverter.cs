// <copyright company="SIX Networks GmbH" file="GamestateConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class GamestateConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;
        static readonly string[] units = {
            "Unknown",
            "Waiting",
            "Unknown",
            "Creating",
            "Unknown",
            "Loading",
            "Briefing",
            "Playing",
            "Unknown",
            "Debriefing"
        };

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DefaultReturn;
            var size = (int) value;
            if (size < 0 || size >= units.Length)
                return DefaultReturn;

            return units[size];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}