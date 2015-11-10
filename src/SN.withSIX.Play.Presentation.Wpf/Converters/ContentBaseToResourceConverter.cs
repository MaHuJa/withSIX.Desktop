// <copyright company="SIX Networks GmbH" file="ContentBaseToResourceConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class ContentBaseToResourceConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture) {
            var strings = value.OfType<string>();
            var image = strings.FirstOrDefault(x => x != null);

            var par = 0;
            var pars = parameter as string;
            if (pars != null)
                par = pars.TryInt();

            return ContentBase.GetResourcePath(image, par);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}