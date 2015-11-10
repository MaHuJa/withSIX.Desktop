// <copyright company="SIX Networks GmbH" file="TextParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace SN.withSIX.Play.Core.Connect.Parser
{
    public class TextParser
    {
        public static IEnumerable<ChatTokenDto> Parse(string text, IEnumerable<ChatTokenDto> existingTokens) {
            existingTokens = existingTokens.OrderBy(x => x.StartIndex);

            var results = new List<ChatTokenDto>();

            var currentIndex = 0;
            foreach (var et in existingTokens) {
                if (et.StartIndex > currentIndex) {
                    results.Add(new ChatTokenDto {
                        StartIndex = currentIndex,
                        EndIndex = et.StartIndex - 1,
                        Content = text.Substring(currentIndex, et.StartIndex - currentIndex),
                        TokenType = ChatTokenType.Text
                    });
                }

                currentIndex = et.EndIndex + 1;
            }

            if (currentIndex < text.Length) {
                results.Add(new ChatTokenDto {
                    StartIndex = currentIndex,
                    EndIndex = text.Length - 1,
                    Content = text.Substring(currentIndex, text.Length - currentIndex),
                    TokenType = ChatTokenType.Text
                });
            }

            return results;
        }
    }
}