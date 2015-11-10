// <copyright company="SIX Networks GmbH" file="KeyValuesTokenizer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;

namespace SN.withSIX.Play.Core.Games.Legacy.Steam
{
    public enum TokenType
    {
        BlockBegin,
        BlockEnd,
        String
    }

    public class KeyValuesTokenizer
    {
        static readonly char[] braces = {'{', '}'};
        static readonly char[] ws = {' ', '\n', '\t'};
        readonly string _data;
        long _line = 1;
        int _position;
        protected int LastLineBreak;

        public KeyValuesTokenizer(string data) {
            _data = data;
        }

        public Tuple<TokenType, string> NextToken() {
            while (true) {
                IgnoreWhitespace();
                if (!IgnoreComment())
                    break;
            }

            var current = Current();
            if (current == default(char))
                return null;

            if (current == '{') {
                Forward();
                return Tuple.Create(TokenType.BlockBegin, (string) null);
            }
            if (current == '}') {
                Forward();
                return Tuple.Create(TokenType.BlockEnd, (string) null);
            }
            return Tuple.Create(TokenType.String, GetString());
        }

        string GetString() {
            var escape = false;
            var r = String.Empty;
            var quoted = false;
            var current = Current();
            if (current == '\"') {
                quoted = true;
                Forward();
            }
            while (true) {
                current = Current();
                if (current == default(char))
                    break;

                if (!quoted && braces.Contains(current))
                    break;

                if (!escape && quoted && current == '\"')
                    break;

                if (escape) {
                    escape = false;
                    if (current == '\"')
                        r += "\"";
                    else if (current == '\\')
                        r += "\\";
                } else if (current == '\\')
                    escape = true;
                else
                    r += current;
                Forward();
            }

            if (quoted)
                Forward();

            return r;
        }

        bool IgnoreComment() {
            var current = Current();
            var next = Next();

            // Skip // comments - TODO: What about storing them??
            if (current == '/' && next == '/') {
                while (Current() != '\n')
                    Forward();
                return true;
            }

            // Skip /* comments */ - TODO: What about storing them?
            // Actually, these aren't supported in the original format
            if (current == '/' && next == '*') {
                while (current != default(char)
                       && (current != '*' && next != '/')) {
                    Forward();
                    current = Current();
                    next = Next();
                    if (current == '\n') {
                        LastLineBreak = _position;
                        _line++;
                    }
                }
                Forward(2); // Move past the */
                return true;
            }

            return false;
        }

        void IgnoreWhitespace() {
            var current = Current();
            while (current != default(char)) {
                if (current == '\n') {
                    LastLineBreak = _position;
                    _line++;
                } else if (!ws.Contains(current))
                    return;
                Forward();
                current = Current();
            }
        }

        bool Forward(int count = 1) {
            return (_position += count) < _data.Length;
        }

        public string Location() {
            return string.Format("line {0}, column {1}", _line, _position - LastLineBreak);
        }

        char Next() {
            var pos = _position + 1;
            if (pos > _data.Length)
                return default(char);
            return _data[pos];
        }

        char Current() {
            if (_position >= _data.Length)
                return default(char);

            return _data[_position];
        }
    }
}