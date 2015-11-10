// <copyright company="SIX Networks GmbH" file="MetaDataDtoBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    [DataContract, DoNotObfuscateType]
    public abstract class MetaDataDtoBase
    {
        [DataMember, JsonProperty("name")]
        public string Name { get; set; }
        [DataMember, JsonProperty("version")]
        public Version Version { get; set; }
        [DataMember, JsonProperty("date"), JsonConverter(typeof (ShortDateConverter))]
        public DateTime? Date { get; set; }
        [DataMember, JsonProperty("release_type")]
        public string ReleaseType { get; set; }
        [DataMember, JsonProperty("full_name")]
        public string FullName { get; set; }
        [DataMember, JsonProperty("description")]
        public string Description { get; set; }
        [DataMember, JsonProperty("summary")]
        public string Summary { get; set; }
        [DataMember, JsonProperty("homepage")]
        public string Homepage { get; set; }
        [DataMember, JsonProperty("authors")]
        public Dictionary<string, string> Authors { get; set; }
        [DataMember, JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [DataMember, JsonProperty("dependencies")]
        public Dictionary<string, string> Dependencies { get; set; }
    }
}