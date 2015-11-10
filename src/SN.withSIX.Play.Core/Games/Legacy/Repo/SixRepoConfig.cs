// <copyright company="SIX Networks GmbH" file="SixRepoConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Play.Core.Games.Legacy.Repo
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Sync.Core.Models.Repositories.SixSync")]
    public class SixRepoConfig : IBaseYaml
    {
        public SixRepoConfig() {
            Apps = new Dictionary<string, SixRepoApp>();
            Mods = new Dictionary<string, SixRepoMod>();
            Missions = new Dictionary<string, string>();
            MPMissions = new Dictionary<string, string>();
            Hosts = new Uri[0];
        }

        [DataMember]
        public Dictionary<string, SixRepoApp> Apps { get; private set; }
        [DataMember]
        public Dictionary<string, string> MPMissions { get; private set; }
        [DataMember]
        public Dictionary<string, string> Missions { get; private set; }
        [DataMember]
        public Dictionary<string, SixRepoMod> Mods { get; private set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Homepage { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string Uuid { get; set; }
        [DataMember]
        public string ImageLarge { get; set; }
        [DataMember]
        public string ServerModsPath { get; set; }
        [DataMember]
        public string ArchiveFormat { get; set; }
        [DataMember]
        public Uri[] Hosts { get; set; }
        [DataMember]
        public string[] Servers { get; set; }
        [DataMember]
        public string[] Include { get; set; }
        [DataMember]
        public int? MaxThreads { get; set; }

        public string ToYaml() {
            var graph = new Dictionary<string, object> {
                {":name", Name},
                {":uuid", Uuid},
                {":homepage", Homepage},
                {":server_mods_path", ServerModsPath},
                {":archive_format", ArchiveFormat},
                {":max_threads", MaxThreads},
                {":hosts", Hosts},
                {":image", Image},
                {":image_large", ImageLarge},
                {":servers", Servers},
                {":missions", Missions},
                {":mpmissions", MPMissions},
                {":mods", Mods},
                {":apps", Apps}
            };
            return graph._ToYaml();
        }

        public void FromYaml(YamlMappingNode mapping) {
            foreach (var entry in mapping.Children) {
                var key = ((YamlScalarNode) entry.Key).Value;
                switch (key) {
                case ":name":
                    Name = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":max_threads":
                    MaxThreads = YamlExtensions.GetIntOrDefault(entry.Value);
                    break;
                case ":uuid":
                    Uuid = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":homepage":
                    Homepage = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":server_mods_path":
                    ServerModsPath = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":archive_format":
                    ArchiveFormat = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":hosts":
                    var hosts = YamlExtensions.GetStringArray(entry.Value);
                    var invalidHost =
                        hosts.FirstOrDefault(x => x == null || !Uri.IsWellFormedUriString(x, UriKind.Absolute));
                    if (invalidHost != null) {
                        throw new InvalidHostFoundException(
                            string.Format("Malformed host found, does it contain valid protocol like 'http://'?\n{0}",
                                invalidHost));
                    }
                    Hosts = hosts.Select(x => x.ToUri()).ToArray();
                    break;
                case ":missions":
                    Missions = YamlExtensions.GetStringDictionary(entry.Value);
                    break;
                case ":mpmissions":
                    MPMissions = YamlExtensions.GetStringDictionary(entry.Value);
                    break;
                case ":image":
                    Image = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":image_large":
                    ImageLarge = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":servers":
                    Servers = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":mods":
                    Mods = GetModDictionary(entry.Value) ?? new Dictionary<string, SixRepoMod>();
                    break;
                case ":apps":
                    Apps = GetAppDictionary(entry.Value) ?? new Dictionary<string, SixRepoApp>();
                    break;
                }
            }
        }

        public string PrettyPrint() {
            throw new NotImplementedException();
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context) {
            if (Apps == null)
                Apps = new Dictionary<string, SixRepoApp>();
            if (Mods == null)
                Mods = new Dictionary<string, SixRepoMod>();
            if (Missions == null)
                Missions = new Dictionary<string, string>();
            if (MPMissions == null)
                MPMissions = new Dictionary<string, string>();
            if (Hosts == null)
                Hosts = new Uri[0];
        }

        public static Dictionary<string, SixRepoApp> GetAppDictionary(YamlNode node) {
            var mapping = node as YamlMappingNode;
            if (mapping == null) {
                var mapping2 = node as YamlScalarNode;
                if (mapping2 != null && String.IsNullOrEmpty(mapping2.Value))
                    return null;

                throw new YamlExpectedOtherNodeTypeException("Expected YamlMappingNode");
            }

            return mapping
                .Select(
                    x => {
                        var newFromYaml = YamlExtensions.NewFromYaml< SixRepoApp>((YamlMappingNode) x.Value);
                        newFromYaml.Name = x.Key.ToString();
                        return new KeyValuePair<string, SixRepoApp>(x.Key.ToString(),
                            newFromYaml);
                    })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<string, SixRepoMod> GetModDictionary(YamlNode node) {
            var mapping = node as YamlMappingNode;
            if (mapping == null) {
                var mapping2 = node as YamlScalarNode;
                if (mapping2 != null && String.IsNullOrEmpty(mapping2.Value))
                    return null;

                throw new YamlExpectedOtherNodeTypeException("Expected YamlMappingNode");
            }

            return mapping.ToDictionary(x => x.Key.ToString(), x => YamlExtensions.NewFromYaml< SixRepoMod>((YamlMappingNode) x.Value));
        }
    }

    [DoNotObfuscate]
    public class InvalidHostFoundException : Exception
    {
        public InvalidHostFoundException(string message) : base(message) {}
    }
}