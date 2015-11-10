// <copyright company="SIX Networks GmbH" file="JsonDictionary.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SN.withSIX.Core.Services
{
    [Serializable]
    public class JsonDictionary<T> : ISerializable where T : class
    {
        public Dictionary<string, T> dict;

        public JsonDictionary(Dictionary<string, T> input = null) {
            if (input == null)
                input = new Dictionary<string, T>();
            dict = input;
        }

        protected JsonDictionary(SerializationInfo info, StreamingContext context) {
            dict = new Dictionary<string, T>();
            foreach (var entry in info) {
                //Debug.Assert(entry.ObjectType.IsArray);
                var array = entry.Value as T;
                dict.Add(entry.Name, array);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            foreach (var key in dict.Keys)
                info.AddValue(key, dict[key]);
        }
    }
}