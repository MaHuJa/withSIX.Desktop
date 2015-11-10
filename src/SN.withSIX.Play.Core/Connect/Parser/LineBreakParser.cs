// <copyright company="SIX Networks GmbH" file="LineBreakParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SN.withSIX.Play.Core.Connect.Parser
{
    public class LineBreakParser
    {
        static readonly Regex lineBreakData = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

        public static IEnumerable<ChatTokenDto> Parse(string text) {
            var results = new List<ChatTokenDto>();

            var matches = lineBreakData.Matches(text);

            if (matches.Count > 0) {
                foreach (Match m in matches) {
                    results.Add(new ChatTokenDto {
                        StartIndex = m.Index,
                        EndIndex = m.Index + m.Length - 1,
                        TokenType = ChatTokenType.LineBreak
                    });
                }
            }


            return results;
        }
    }
}