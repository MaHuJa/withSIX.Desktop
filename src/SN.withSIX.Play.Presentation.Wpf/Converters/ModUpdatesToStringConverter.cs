// <copyright company="SIX Networks GmbH" file="ModUpdatesToStringConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Presentation.Wpf.Converters
{
    public class ModUpdatesToStringConverter : IValueConverter
    {
        static readonly string DefaultReturn = String.Empty;
        static readonly string Concat = "\n";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DefaultReturn;

            var collection = (IEnumerable<UpdateState>) value;
            return String.Join(Concat, collection
                .Select(mu => String.Format("{0}: {1} (Current: {2}, {3}) Total Size: {4}, Compressed: {5}",
                    mu.Mod.Name, mu.Revision, mu.CurrentRevision ?? "None", GetState(mu),
                    Tools.FileUtil.GetFileSize(mu.SizeWd), Tools.FileUtil.GetFileSize(mu.Size))));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        static string GetState(UpdateState mu) {
            if (string.IsNullOrWhiteSpace(mu.CurrentRevision))
                return "New";
            if (mu.IsEqual())
                return "Diagnose";
            return mu.IsNewer() ? "Upgrade" : "Downgrade";
        }

        #endregion
    }
}