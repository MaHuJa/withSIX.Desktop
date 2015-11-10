// <copyright company="SIX Networks GmbH" file="PackageMetaDataDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    [DataContract, DoNotObfuscateType]
    public class PackageMetaDataDto : MetaDataDtoBase
    {
        [DataMember, JsonProperty("branch")]
        public string Branch { get; set; }
        [DataMember, JsonProperty("content_type")]
        public string ContentType { get; set; }
        [DataMember, JsonProperty("size")]
        public long Size { get; set; }
        [DataMember, JsonProperty("size_packed")]
        public long SizePacked { get; set; }
        [DataMember, JsonProperty("licenses")]
        public List<string> Licenses { get; set; }
        [DataMember, JsonProperty("additional")]
        public Dictionary<string, string> Additional { get; set; }
        [DataMember, JsonProperty("files")]
        public Dictionary<string, string> Files { get; set; }
    }
}