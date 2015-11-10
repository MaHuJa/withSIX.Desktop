// <copyright company="SIX Networks GmbH" file="ModDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Infra.Api.ContentApi.Dto
{
    [DataContract, DoNotObfuscateType]
    class ModDto : NewContentBaseDto
    {
        [DataMember]
        public List<CategoryRelationDto> Categories { get; set; }
        [DataMember]
        public string Aliases { get; set; }
        [DataMember]
        public string CppName { get; set; }
        [DataMember]
        public string PackageName { get; set; }
        [DataMember]
        public bool HasImage { get; set; }
        [DataMember]
        public List<string> Dependencies { get; set; }
        [DataMember, JsonConverter(typeof (LockedGlobalizationDateConverter))]
        public DateTime UpdatedVersion { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string MinBuild { get; set; }
        [DataMember]
        public string MaxBuild { get; set; }
        [DataMember]
        public string Guid { get; set; }
        [DataMember]
        public Guid GameId { get; set; }
        [DataMember]
        public bool HasLicense { get; set; }
        [DataMember]
        public string ModVersion { get; set; }
        [DataMember]
        public long Size { get; set; }
        [DataMember]
        public long SizeWd { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
    }
}