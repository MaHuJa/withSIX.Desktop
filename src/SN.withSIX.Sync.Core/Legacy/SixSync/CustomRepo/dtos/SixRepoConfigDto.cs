// <copyright company="SIX Networks GmbH" file="SixRepoConfigDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos
{
    public class SixRepoConfigDto
    {
        public SixRepoConfigDto() {
            Apps = new Dictionary<string, SixRepoAppDto>();
            Mods = new Dictionary<string, SixRepoModDto>();
            Missions = new Dictionary<string, string>();
            MPMissions = new Dictionary<string, string>();
            Servers = new List<string>();
            Include = new List<string>();
            Hosts = new List<Uri>();
        }

        [YamlMember(Alias = ":apps")]
        public Dictionary<string, SixRepoAppDto> Apps { get; set; }
        [YamlMember(Alias = ":mpmissions")]
        public Dictionary<string, string> MPMissions { get; set; }
        [YamlMember(Alias = ":missions")]
        public Dictionary<string, string> Missions { get; set; }
        [YamlMember(Alias = ":mods")]
        public Dictionary<string, SixRepoModDto> Mods { get; set; }
        [YamlMember(Alias = ":name")]
        public string Name { get; set; }
        [YamlMember(Alias = ":homepage")]
        public string Homepage { get; set; }
        [YamlMember(Alias = ":image")]
        public string Image { get; set; }
        [YamlMember(Alias = ":uuid")]
        public string Uuid { get; set; }
        [YamlMember(Alias = ":image_large")]
        public string ImageLarge { get; set; }
        [YamlMember(Alias = ":server_mods_path")]
        public string ServerModsPath { get; set; }
        [YamlMember(Alias = ":archive_format")]
        public string ArchiveFormat { get; set; }
        [YamlMember(Alias = ":hosts")]
        public List<Uri> Hosts { get; set; }
        [YamlMember(Alias = ":servers")]
        public List<string> Servers { get; set; }
        [YamlMember(Alias = ":include")]
        public List<string> Include { get; set; }
        [YamlMember(Alias = ":max_threads")]
        public int? MaxThreads { get; set; }
    }
}