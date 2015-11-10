// <copyright company="SIX Networks GmbH" file="BooleansToVisibilityCollapsedConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class BooleansToVisibilityCollapsedConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var inverse = parameter != null && ((string) parameter).TryBool();
            var bools = values.OfType<bool>().ToArray();
            var val = inverse ? bools.None(x => x) : bools.None(x => !x);

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}