// <copyright company="SIX Networks GmbH" file="SixRepoAppDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using YamlDotNet.Serialization;

namespace SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos
{
    public enum AppType
    {
        Teamspeak3,
        Mumble
    }

    public class SixRepoAppDto
    {
        [YamlMember(Alias = ":channel")]
        public string Channel { get; set; }
        [YamlMember(Alias = ":channel_password")]
        public string ChannelPassword { get; set; }
        [YamlMember(Alias = ":name")]
        public string Name { get; set; }
        [YamlMember(Alias = ":hidden")]
        public bool IsHidden { get; set; }
        [YamlMember(Alias = ":ip")]
        public string Ip { get; set; }
        [YamlMember(Alias = ":port")]
        public int Port { get; set; }
        [YamlMember(Alias = ":password")]
        public string Password { get; set; }
        [YamlMember(Alias = ":type")]
        public AppType Type { get; set; }
    }
}