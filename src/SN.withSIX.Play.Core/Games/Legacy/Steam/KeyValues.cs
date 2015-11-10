// <copyright company="SIX Networks GmbH" file="KeyValues.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy.Steam
{
    [DoNotObfuscate]
    public class ParseException : Exception
    {
        public ParseException() {}
        public ParseException(string message) : base(message) {}
    }

    // Support for Valve Data Format: https://developer.valvesoftware.com/wiki/KeyValues
    public class KeyValues : Dictionary<string, object>
    {
        public KeyValues() {}
        public KeyValues(IDictionary<string, object> input) : base(input) {}

        public KeyValues(string readAllText) {
            Load(readAllText);
        }

        public string GetString(string key) {
            return (string) this[key];
        }

        public string GetString(IEnumerable<string> keysIn) {
            var keys = keysIn.ToArray();
            var last = keys.Last();
            var entry = this;
            foreach (var k in keys.Take(keys.Length - 1)) {
                var kv = entry.GetKeyValue(k);
                entry = kv;
            }
            return entry.GetString(last);
        }

        public KeyValues GetKeyValue(string key) {
            return (KeyValues) this[key];
        }

        public KeyValues GetKeyValue(IEnumerable<string> keysIn) {
            var keys = keysIn.ToArray();
            var last = keys.Last();
            var entry = this;
            foreach (var k in keys.Take(keys.Length - 1)) {
                var kv = entry.GetKeyValue(k);
                entry = kv;
            }
            return entry.GetKeyValue(last);
        }

        public string PrettyPrint(int level = 0) {
            var sb = new StringBuilder();
            var nextLevel = level + 1;
            var indent = new string('\t', level);
            foreach (var kvp in this) {
                if (kvp.Value is string)
                    sb.AppendFormat("{0}\"{1}\" \"{2}\"\n", indent, kvp.Key, kvp.Value);
                else if (kvp.Value is KeyValues) {
                    var kv = kvp.Value as KeyValues;
                    sb.AppendFormat("{0}\"{1}\"\n{0}{{\n{2}{0}}}\n", indent, kvp.Key, kv.PrettyPrint(nextLevel));
                }
            }

            return sb.ToString();
        }

        public void Load(string data) {
            var tokenizer = new KeyValuesTokenizer(data);
            var token = tokenizer.NextToken();
            if (token == null || token.Item1 != TokenType.String)
                throw new ParseException("Invalid token at " + tokenizer.Location());

            var key = token.Item2;
            token = tokenizer.NextToken();
            if (token == null || token.Item1 != TokenType.BlockBegin) {
                throw new ParseException(String.Format("Invalid token: {0}, {1} at {2}", token.Item1, token.Item2,
                    tokenizer.Location()));
            }

            var kv = new KeyValues();
            this[key] = kv;
            kv.Parse(tokenizer);

            token = tokenizer.NextToken();
            if (token != null)
                throw new ParseException("Unexpected token at file end");
        }

        void Parse(KeyValuesTokenizer tokenizer) {
            string key = null;

            while (true) {
                var token = tokenizer.NextToken();
                if (token == null)
                    throw new ParseException("Unexpected end of file");

                if (key != null) {
                    if (token.Item1 == TokenType.BlockBegin) {
                        var value = new KeyValues();
                        value.Parse(tokenizer);
                        this[key] = value;
                    } else if (token.Item1 == TokenType.String)
                        this[key] = token.Item2;
                    else
                        throw new ParseException("Invalid token at " + tokenizer.Location());
                    key = null;
                } else {
                    if (token.Item1 == TokenType.BlockEnd)
                        break;
                    if (token.Item1 != TokenType.String)
                        throw new ParseException("Invalid token at " + tokenizer.Location());
                    key = token.Item2;
                }
            }
        }
    }
}