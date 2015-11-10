// <copyright company="SIX Networks GmbH" file="EnumToObjectConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    [ContentProperty("Items")]
    public class EnumToObjectConverter : IValueConverter
    {
        public ResourceDictionary Items { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var key = Enum.GetName(value.GetType(), value);
            return Items[key];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("This converter only works for one way binding");
        }
    }

    [ContentProperty("Items")]
    public class EnumToClonedObjectConverter : IValueConverter
    {
        public ResourceDictionary Items { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var key = Enum.GetName(value.GetType(), value);
            var stringReader = new StringReader(XamlWriter.Save(Items[key]));
            var xmlReader = XmlReader.Create(stringReader);
            return XamlReader.Load(xmlReader);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("This converter only works for one way binding");
        }
    }
}