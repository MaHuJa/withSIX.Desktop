// <copyright company="SIX Networks GmbH" file="ChatTokenizer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace SN.withSIX.Play.Core.Connect.Parser
{
    public enum ChatTokenType
    {
        Text,
        LineBreak,
        Image,
        Link
    }

    public class ChatTokenDto
    {
        public string Content;
        public int EndIndex;
        public int StartIndex;
        public ChatTokenType TokenType;
    }

    public class ChatTokenizer
    {
        public static IEnumerable<ChatTokenDto> Tokenize(string chatText) {
            var tokens = new List<ChatTokenDto>();

            tokens.AddRange(SmileyParser.Parse(chatText));
            tokens.AddRange(LinkParser.Parse(chatText));
            tokens.AddRange(LineBreakParser.Parse(chatText));

            tokens.AddRange(TextParser.Parse(chatText, tokens));

            tokens = tokens.OrderBy(x => x.StartIndex).ToList();
            return tokens;
        }
    }
}