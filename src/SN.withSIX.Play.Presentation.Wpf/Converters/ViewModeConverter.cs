// <copyright company="SIX Networks GmbH" file="ViewModeConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using SN.withSIX.Core.Applications;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class ViewModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((ViewType) value) {
            case ViewType.Grid: {
                return SixIconFont.withSIX_icon_Display_List;
            }
            case ViewType.List: {
                return SixIconFont.withSIX_icon_Display_Grid;
            }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}