// <copyright company="SIX Networks GmbH" file="CategoryDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Infra.Api.ContentApi.Dto
{
    [DataContract, DoNotObfuscateType]
    class CategoryDto : NewSyncBaseDto
    {
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract, DoNotObfuscateType]
    class CategoryRelationDto : NewRelationDtoBase {}
}