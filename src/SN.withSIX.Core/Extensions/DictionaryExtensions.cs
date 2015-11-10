// <copyright company="SIX Networks GmbH" file="DictionaryExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;

namespace SN.withSIX.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static string PrettyPrint<TK, TV>(this IDictionary<TK, TV> dict) {
            if (dict == null)
                return String.Empty;
            var dictStr = "[";
            var keys = dict.Keys;
            var i = 0;
            foreach (var key in keys) {
                dictStr += key + "=" + PrintValue(dict[key]);
                if (i++ < keys.Count - 1)
                    dictStr += ",\n";
            }
            return dictStr + "]";
        }

        static string PrintValue(object o) {
            if (o == null)
                return "<null>";

            return o is string ? "\"" + o + "\"" : o.ToString();
        }

        public static void RenameKey<TK, TV>(this IDictionary<TK, TV> dict,
            TK fromKey, TK toKey) {
            var value = dict[fromKey];
            dict.Remove(fromKey);
            dict[toKey] = value;
        }

        public static void AddOrOverwrite<TK, TV>(this IDictionary<TK, TV> dict,
            TK key, TV value) {
            TV existingValue;
            if (dict.TryGetValue(key, out existingValue))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
    }
}