// <copyright company="SIX Networks GmbH" file="CountExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions
{
    public static class CountExtensions
    {
        public static string FormatCount(this int count, string type) {
            return string.Format("{0} {1}", count, type.LarizeAsNeeded(count));
        }

        public static string LarizeAsNeeded(this string str, int count) {
            return count == 1 ? str.Singularize() : str.Pluralize();
        }

        public static string Singularize(this string str) {
            if (str.EndsWith("ies"))
                return str.Substring(0, str.Length - 3);

            if (str.EndsWith("s"))
                return str.Substring(0, str.Length - 1);

            return str;
        }
    }
}