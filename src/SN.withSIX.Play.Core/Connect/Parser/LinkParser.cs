// <copyright company="SIX Networks GmbH" file="LinkParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SN.withSIX.Play.Core.Connect.Parser
{
    public class LinkParser
    {
        public static readonly Regex LinkData = new Regex(
            @"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+\!\(\)=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)",
            RegexOptions.Compiled);

        public static IEnumerable<ChatTokenDto> Parse(string text) {
            var results = new List<ChatTokenDto>();

            var matches = LinkData.Matches(text);

            if (matches.Count > 0) {
                foreach (Match m in matches) {
                    results.Add(new ChatTokenDto {
                        StartIndex = m.Index,
                        EndIndex = m.Index + m.Length - 1,
                        Content = m.Value,
                        TokenType = ChatTokenType.Link
                    });
                }
            }

            return results;
        }
    }
}