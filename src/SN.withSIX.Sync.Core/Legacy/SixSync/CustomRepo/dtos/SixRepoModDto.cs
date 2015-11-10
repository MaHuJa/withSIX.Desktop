// <copyright company="SIX Networks GmbH" file="SixRepoModDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos
{
    public class SixRepoModDto
    {
        public SixRepoModDto() {
            Categories = new List<string>();
            Dependencies = new List<string>();
            Aliases = new List<string>();
        }

        [YamlMember(Alias = ":image")]
        public string Image { get; set; }
        [YamlMember(Alias = ":image_large")]
        public string ImageLarge { get; set; }
        [YamlMember(Alias = ":author")]
        public string Author { get; set; }
        [YamlMember(Alias = ":categories")]
        public List<string> Categories { get; set; }
        [YamlMember(Alias = ":version")]
        public long Version { get; set; }
        [YamlMember(Alias = ":dependencies")]
        public List<string> Dependencies { get; set; }
        [YamlMember(Alias = ":size")]
        public long Size { get; set; }
        [YamlMember(Alias = ":wd_size")]
        public long WdSize { get; set; }
        [YamlMember(Alias = ":aliases")]
        public List<string> Aliases { get; set; }
        [YamlMember(Alias = ":full_name")]
        public string FullName { get; set; }
        [YamlMember(Alias = ":cpp_name")]
        public string CppName { get; set; }
        [YamlMember(Alias = ":description")]
        public string Description { get; set; }
        [YamlMember(Alias = ":homepage")]
        public string Homepage { get; set; }
        [YamlMember(Alias = ":guid")]
        public string Guid { get; set; }
        [YamlMember(Alias = ":license")]
        public string License { get; set; }
        [YamlMember(Alias = ":license_url")]
        public string LicenseUrl { get; set; }
        [YamlMember(Alias = ":updated_version")]
        public DateTime UpdatedVersion { get; set; }
        [YamlMember(Alias = ":mod_version")]
        public string ModVersion { get; set; }
        [YamlMember(Alias = ":type")]
        public string Type { get; set; }
    }
}