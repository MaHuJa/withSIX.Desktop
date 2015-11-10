// <copyright company="SIX Networks GmbH" file="RepoConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    public class RepoConfig : IBaseYaml //, IYamlSerializable
    {
        public RepoConfig() {
            Hosts = new Uri[0];
            Exclude = new string[0];
            Include = new string[0];
        }

        public string PackPath { get; set; }
        public Uri[] Hosts { get; set; }
        public string[] Exclude { get; set; }
        public string[] Include { get; set; }

        public string ToYaml() {
            var graph = new Dictionary<string, object> {
                {":pack_path", PackPath},
                {":exclude", Exclude},
                {":include", Include},
                {":hosts", Hosts.Select(x => x.ToString())}
            };
            return graph._ToYaml();
        }

        public void FromYaml(YamlMappingNode mapping) {
            foreach (var entry in mapping.Children) {
                var key = ((YamlScalarNode) entry.Key).Value;
                switch (key) {
                case ":pack_path":
                    PackPath = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":hosts":
                    Hosts = YamlExtensions.GetStringArray(entry.Value).Select(x => new Uri(x)).ToArray();
                    break;
                case ":exclude":
                    Exclude = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":include":
                    Include = YamlExtensions.GetStringArray(entry.Value);
                    break;
                }
            }
        }

        public string PrettyPrint() {
            return
                String.Format("PackPath: {0}\nHosts: {1}", PackPath, String.Join(", ", Hosts.Select(x => x.ToString())));
        }
    }
}