// <copyright company="SIX Networks GmbH" file="Matcher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Text.RegularExpressions;

namespace Mac.Arma.Misc
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Values that represent ways of matching. </summary>
    /// <remarks>
    ///     Structures within Mac.Arma can be 'matched' against patters
    ///     in a variety of ways ranging from pure regular expressions to
    ///     literal matches.
    ///     MatchType.Literal means that the pattern is treated as a literal string
    ///     (although non case-sensitive)
    ///     MatchType.Regex means that the pattern is treated as a .Net Regex
    ///     MatchType.Path means that the pattern is treated in a similar way to
    ///     wildcarded matches in DOS commands. Ie, *.x will match anything that ends
    ///     with .x
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public enum MatchType
    {
        /// <summary>Match against a literal pattern</summary>
        Literal,

        /// <summary> Match against a regular expression</summary>
        Regex,

        /// <summary> Match against a DOS wildcard path</summary>
        Path
    }

    static class Matcher
    {
        public static string MatchString(string pattern, MatchType type) {
            switch (type) {
            case MatchType.Literal:
                pattern = "^" + pattern + "$";
                return Regex.Escape(pattern);
            case MatchType.Path:
                if (pattern.StartsWith(@"\", StringComparison.Ordinal))
                    pattern = pattern.Substring(1);
                pattern = pattern.Replace(@"\", @"\\");
                pattern = pattern.Replace(".", @"\.");
                pattern = pattern.Replace("*", ".*");
                pattern = "^" + pattern + "$";
                return pattern;
            case MatchType.Regex:
                return pattern;
            default:
                throw new NotImplementedException();
            }
        }
    }
}