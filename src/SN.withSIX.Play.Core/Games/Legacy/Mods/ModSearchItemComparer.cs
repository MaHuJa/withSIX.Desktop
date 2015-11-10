// <copyright company="SIX Networks GmbH" file="ModSearchItemComparer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public class ModSearchItemComparer : IEqualityComparer<IContent>, IEqualityComparer<IMod>
    {
        public bool Equals(IContent x, IContent y) {
            if (x == null || y == null)
                return false;

            var xm = x.ToMod();
            var ym = y.ToMod();
            if (xm != null && ym != null)
                return xm.ComparePK(ym);

            return x.Equals(y);
        }

        public int GetHashCode(IContent obj) {
            return ReferenceEquals(obj, null) ? 0 : obj.Name.GetHashCode();
        }

        public bool Equals(IMod x, IMod y) {
            return x.ComparePK(y);
        }

        public int GetHashCode(IMod obj) {
            return ReferenceEquals(obj, null) ? 0 : obj.Name.GetHashCode();
        }
    }
}