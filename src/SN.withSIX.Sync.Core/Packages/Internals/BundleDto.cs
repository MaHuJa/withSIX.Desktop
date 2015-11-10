// <copyright company="SIX Networks GmbH" file="BundleDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    [DataContract, DoNotObfuscateType]
    public class BundleDto : MetaDataDtoBase
    {
        [DataMember, JsonProperty("required")]
        public Dictionary<string, string> Required { get; set; }
        [DataMember, JsonProperty("required_clients")]
        public Dictionary<string, string> RequiredClients { get; set; }
        [DataMember, JsonProperty("required_servers")]
        public Dictionary<string, string> RequiredServers { get; set; }
        [DataMember, JsonProperty("optional")]
        public Dictionary<string, string> Optional { get; set; }
        [DataMember, JsonProperty("optional_clients")]
        public Dictionary<string, string> OptionalClients { get; set; }
        [DataMember, JsonProperty("optional_servers")]
        public Dictionary<string, string> OptionalServers { get; set; }
        [DataMember, JsonProperty("additional")]
        public Dictionary<string, string> Additional { get; set; }
    }
}