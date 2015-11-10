// <copyright company="SIX Networks GmbH" file="MissionTypeConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    class MissionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var type = value as String;
            if (string.IsNullOrWhiteSpace(type))
                return "Unknown";

            return type.Equals(MissionTypes.MpMission) ? MissionTypesHunan.MpMission : MissionTypesHunan.SpMission;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}