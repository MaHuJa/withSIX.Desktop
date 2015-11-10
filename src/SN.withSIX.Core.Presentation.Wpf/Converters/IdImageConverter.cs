// <copyright company="SIX Networks GmbH" file="IdImageConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class IdImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;

            var obj = (IHaveId<Guid>) value;
            var infos = ((string) parameter).Split(',');

            // TODO: UserContentCDN?
            return Tools.Transfer.JoinUri(CommonUrls.ImageCdn, infos[0], obj.Id + "-" + infos[1] + ".png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}