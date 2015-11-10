// <copyright company="SIX Networks GmbH" file="RepoVersion.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    public class RepoVersion : IBaseYaml
    {
        public Dictionary<string, string> Pack = new Dictionary<string, string>();
        public Dictionary<string, string> WD = new Dictionary<string, string>();

        public RepoVersion() {
            ArchiveFormat = Repository.DefaultArchiveFormat;
            FormatVersion = 1;
            Pack = new Dictionary<string, string>();
            WD = new Dictionary<string, string>();
        }

        public string ArchiveFormat { get; set; }
        public long Version { get; set; }
        public long FormatVersion { get; set; }
        public string Guid { get; set; }
        public long PackSize { get; set; }
        public long WdSize { get; set; }

        public string ToYaml() {
            var graph = new Dictionary<string, object> {
                {":archive_format", ArchiveFormat},
                {":format_version", FormatVersion},
                {":guid", Guid},
                {":version", Version},
                {":pack_size", PackSize},
                {":wd_size", WdSize},
                {":pack", Pack},
                {":wd", WD}
            };
            return graph._ToYaml();
        }

        public void FromYaml(YamlMappingNode mapping) {
            foreach (var entry in mapping.Children) {
                var key = ((YamlScalarNode) entry.Key).Value;
                switch (key) {
                case ":archive_format":
                    ArchiveFormat = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":format_version":
                    FormatVersion = YamlExtensions.GetLongOrDefault(entry.Value);
                    break;
                case ":guid":
                    Guid = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":version":
                    Version = YamlExtensions.GetLongOrDefault(entry.Value);
                    break;
                case ":pack_size":
                    PackSize = YamlExtensions.GetLongOrDefault(entry.Value);
                    break;
                case ":wd_size":
                    WdSize = YamlExtensions.GetLongOrDefault(entry.Value);
                    break;
                case ":pack":
                    Pack = YamlExtensions.GetStringDictionary(entry.Value);
                    break;
                case ":wd":
                    WD = YamlExtensions.GetStringDictionary(entry.Value);
                    break;
                }
            }
        }

        public string PrettyPrint() {
            return
                String.Format(
                    "Guid: {6}\nVersion: {7}\nArchiveFormat: {0}\nFormatVersion: {1}\nPackSize: {2}\nWdSize: {3}\nPack: {4}\nWD: {5}",
                    ArchiveFormat, FormatVersion, PackSize, WdSize, String.Join(", ", Pack),
                    String.Join(", ", WD), Guid, Version);
        }
    }
}