// <copyright company="SIX Networks GmbH" file="PlayersToStringConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SN.withSIX.Play.Core.Games.Legacy.Servers;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class PlayersToStringConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;
        static readonly string concat = ", ";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DefaultReturn;

            var collection = (IEnumerable) value;
            IList<string> c = collection.Cast<Player>().Select(player => player.Name).ToList();

            return String.Join(concat, c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}