// <copyright company="SIX Networks GmbH" file="IsContentInSetConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class IsContentInSetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length < 2)
                return false;

            var paraString = parameter as string;
            var reverse = paraString != null && paraString.TryBool();

            var set = values[0] as IHaveReactiveItems<IContent>;
            var item = values[1] as IContent;
            if (set == null || item == null)
                return false;

            var contains = TryIsInSet(set, item);
            return reverse ? !contains : contains;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static bool TryIsInSet(IHaveReactiveItems<IContent> set, IContent item) {
            try {
                return IsInSet(set, item);
            } catch (InvalidOperationException e) {
                MainLog.Logger.FormattedWarnException(e);
                return false;
            }
        }

        static bool IsInSet(IHaveReactiveItems<IContent> set, IContent item) {
            return set.Items.Contains(item) ||
                   set.Items.Any(x => x.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}