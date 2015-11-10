// <copyright company="SIX Networks GmbH" file="BaseYaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    public static class YamlExtensions
    {
        public static Dictionary<string, string> ToStringDictionary(IDictionary<YamlNode, YamlNode> list) {
            Contract.Requires<ArgumentNullException>(list != null);

            return list.ToDictionary(ent => ((YamlScalarNode) ent.Key).Value,
                ent => ((YamlScalarNode) ent.Value).Value);
        }

        public static Dictionary<string, int> ToIntDictionary(IDictionary<YamlNode, YamlNode> list) {
            Contract.Requires<ArgumentNullException>(list != null);

            return list.ToDictionary(ent => ((YamlScalarNode) ent.Key).Value,
                ent => ((YamlScalarNode) ent.Value).Value.TryInt());
        }

        public static Dictionary<string, long> ToLongDictionary(IDictionary<YamlNode, YamlNode> list) {
            Contract.Requires<ArgumentNullException>(list != null);

            return list.ToDictionary(ent => ((YamlScalarNode) ent.Key).Value,
                ent => ((YamlScalarNode) ent.Value).Value.TryLong());
        }

        public static Dictionary<string, string> GetStringDictionaryInternal(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            var mapping = node as YamlMappingNode;
            if (mapping == null) {
                var mapping2 = node as YamlScalarNode;
                if (mapping2 != null && String.IsNullOrEmpty(mapping2.Value))
                    return null;

                var mapping3 = node as YamlSequenceNode;
                if (mapping3 != null) {
                    return mapping3
                        .Select(n => ((YamlSequenceNode) n).ToArray())
                        .ToDictionary(ar => ((YamlScalarNode) ar[0]).Value, ar => ((YamlScalarNode) ar[1]).Value);
                }

                throw new YamlExpectedOtherNodeTypeException("Expected YamlMappingNode");
            }
            return ToStringDictionary(mapping.Children);
        }

        public static string[] GetStringArrayInternal(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            var mapping = node as YamlSequenceNode;
            if (mapping == null) {
                var mapping2 = node as YamlScalarNode;
                if (mapping2 != null && String.IsNullOrEmpty(mapping2.Value))
                    return null;
                throw new YamlExpectedOtherNodeTypeException("Expected YamlSequenceNode");
            }
            return mapping.Children.Select(x => ((YamlScalarNode) x).Value).ToArray();
        }

        static YamlNode GetRootNode(YamlStream yaml) {
            Contract.Requires<ArgumentNullException>(yaml != null);

            if (yaml.Documents.Count == 0)
                throw new YamlParseException("Contains no documents");
            return yaml.Documents[0].RootNode;
        }

        public static YamlMappingNode GetMapping(YamlStream yaml) {
            Contract.Requires<ArgumentNullException>(yaml != null);

            var node = GetRootNode(yaml);
            var mapped = node as YamlMappingNode;
            if (mapped == null)
                throw new YamlParseException("Not a mapping");
            return mapped;
        }

        public static DateTime GetDateTimeOrDefault(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            var val = ((YamlScalarNode) node).Value;
            DateTime dt;
            DateTime.TryParse(val, out dt);
            return dt;
        }

        public static string GetStringOrDefault(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            return ((YamlScalarNode) node).Value;
        }

        public static long GetLongOrDefault(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            return ((YamlScalarNode) node).Value.TryLong();
        }

        public static int GetIntOrDefault(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            return ((YamlScalarNode) node).Value.TryInt();
        }

        public static bool GetBoolOrDefault(YamlNode node) {
            Contract.Requires<ArgumentNullException>(node != null);

            return ((YamlScalarNode) node).Value.TryBool();
        }

        public static string[] GetStringArray(YamlNode node) {
            return GetStringArrayInternal(node) ?? new string[0];
        }

        public static Dictionary<string, string> GetStringDictionary(YamlNode node) {
            return GetStringDictionaryInternal(node) ?? new Dictionary<string, string>();
        }

        public static string _ToYaml(this object graph) {
            Contract.Requires<ArgumentNullException>(graph != null);

            var serializer = new Serializer();
            var text = new StringWriter();
            text.Write("--- \r\n");
            var emitter = new Emitter(text);
            serializer.Serialize(emitter, graph);

            return text.ToString();
        }

        public static void SaveYaml(this IBaseYaml yml, IAbsoluteFilePath fileName) {
            YamlIoExtensions._SaveYaml(yml.ToYaml(), fileName);
        }

        public static void FromYaml(this IBaseYaml yml, YamlStream yaml) {
            yml.FromYaml(GetMapping(yaml));
        }

        public static void FromYamlFile(this IBaseYaml yml, IAbsoluteFilePath fileName) {
            yml.FromYaml(GetMapping(fileName.ReadYamlFile()));
        }

        public static void FromYaml(this IBaseYaml yml, string yamlStr) {
            yml.FromYaml(GetMapping(yamlStr.ReadYaml()));
        }

        public static T NewFromYamlFile<T>(IAbsoluteFilePath fileName) where T : IBaseYaml, new() {
            return NewFromYaml<T>(GetMapping(fileName.ReadYamlFile()));
        }

        public static T NewFromYaml<T>(string yaml) where T : IBaseYaml, new() {
            return NewFromYaml<T>(GetMapping(yaml.ReadYaml()));
        }

        public static T NewFromYaml<T>(this T yml, YamlStream stream) where T : IBaseYaml, new() {
            return NewFromYaml<T>(GetMapping(stream));
        }

        public static T NewFromYaml<T>(YamlMappingNode mapping) where T : IBaseYaml, new() {
            var repo = new T();
            repo.FromYaml(mapping);
            return repo;
        }
    }

    public static class YamlIoExtensions
    {
        internal static void _SaveYaml(string yaml, IAbsoluteFilePath fileName) {
            Contract.Requires<ArgumentNullException>(yaml != null);
            Contract.Requires<ArgumentNullException>(fileName != null);

            Tools.FileTools.SafeIO.SafeSave(x => {
                using (var sr = new StreamWriter(x.ToString()))
                    sr.Write(yaml);
            }, fileName);
        }

        internal static YamlStream ReadYaml(this string yaml) {
            Contract.Requires<ArgumentNullException>(yaml != null);

            using (var input = new StringReader(yaml)) {
                var yaml1 = new YamlStream();
                yaml1.Load(input);
                return yaml1;
            }
        }

        internal static YamlStream ReadYamlFile(this IAbsoluteFilePath yaml) {
            Contract.Requires<ArgumentNullException>(yaml != null);

            using (var stream = File.Open(yaml.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var input = new StreamReader(stream)) {
                var yaml1 = new YamlStream();
                yaml1.Load(input);
                return yaml1;
            }
        }
    }
}