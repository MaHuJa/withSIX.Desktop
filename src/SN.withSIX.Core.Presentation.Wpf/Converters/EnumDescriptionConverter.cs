// <copyright company="SIX Networks GmbH" file="EnumDescriptionConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var myEnum = (Enum) value;
            return GetEnumDescription(myEnum);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return string.Empty;
        }

        static string GetEnumDescription(Enum enumObj) {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            var attribArray = fieldInfo.GetCustomAttributes(false);
            var attrib = attribArray.OfType<DescriptionAttribute>().FirstOrDefault();
            return attrib == null ? enumObj.ToString() : attrib.Description;
        }
    }
}