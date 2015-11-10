// <copyright company="SIX Networks GmbH" file="SixRepoServerDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos
{
    public class SixRepoServerDto
    {
        public SixRepoServerDto() {
            Motd = new List<string>();
            Rules = new List<string>();
            RequiredMods = new List<string>();
            AllowedMods = new List<string>();
            Apps = new List<string>();
            Missions = new List<string>();
            MPMissions = new List<string>();
        }

        [YamlMember(Alias = ":game")]
        public string Game { get; set; }
        [YamlMember(Alias = ":image")]
        public string Image { get; set; }
        [YamlMember(Alias = ":image_large")]
        public string ImageLarge { get; set; }
        [YamlMember(Alias = ":name")]
        public string Name { get; set; }
        [YamlMember(Alias = ":uuid")]
        public string Uuid { get; set; }
        [YamlMember(Alias = ":ip")]
        public string Ip { get; set; }
        [YamlMember(Alias = ":port")]
        public int Port { get; set; }
        [YamlMember(Alias = ":info")]
        public string Info { get; set; }
        [YamlMember(Alias = ":password")]
        public string Password { get; set; }
        [YamlMember(Alias = ":motd")]
        public List<string> Motd { get; set; }
        [YamlMember(Alias = ":rules")]
        public List<string> Rules { get; set; }
        [YamlMember(Alias = ":required_mods")]
        public List<string> RequiredMods { get; set; }
        [YamlMember(Alias = ":allowed_mods")]
        public List<string> AllowedMods { get; set; }
        [YamlMember(Alias = ":apps")]
        public List<string> Apps { get; set; }
        [YamlMember(Alias = ":missions")]
        public List<string> Missions { get; set; }
        [YamlMember(Alias = ":mpmissions")]
        public List<string> MPMissions { get; set; }
        [YamlMember(Alias = ":open")]
        public bool IsOpen { get; set; }
        [YamlMember(Alias = ":hidden")]
        public bool IsHidden { get; set; }
        [YamlMember(Alias = ":force_mod_update")]
        public bool ForceModUpdate { get; set; }
        [YamlMember(Alias = ":force_server_name")]
        public bool ForceServerName { get; set; }
    }
}