// <copyright company="SIX Networks GmbH" file="ParameterExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SN.withSIX.Core.Extensions
{
    public static class ParameterExtensions
    {
        static readonly Regex rxSpaces = new Regex(@"^(.*\s.*?)(\\*)$", RegexOptions.Compiled);
        static readonly Regex rxBackslashesAndDoubleQuote = new Regex(@"(\\*)" + "\"", RegexOptions.Compiled);

        public static string CombineParameters(this IEnumerable<string> startupParams) {
            return string.Join(" ", startupParams.Select(EscapeArgumentWhenContainsSpacesOrDoubleQuotes));
        }

        public static string CombineParameters(params string[] startupParams) {
            return CombineParameters((IEnumerable<string>) startupParams);
        }

        static string EscapeArgumentWhenContainsSpacesOrDoubleQuotes(string original) {
            return string.IsNullOrEmpty(original)
                ? original
                : HandleSpaces(ReplaceBackslashesAndDoubleQuotes(original));
        }

        static string HandleSpaces(string value) {
            return rxSpaces.Replace(value, "\"$1$2$2\"");
        }

        static string ReplaceBackslashesAndDoubleQuotes(string original) {
            return rxBackslashesAndDoubleQuote.Replace(original, @"$1\$0");
        }
    }
}