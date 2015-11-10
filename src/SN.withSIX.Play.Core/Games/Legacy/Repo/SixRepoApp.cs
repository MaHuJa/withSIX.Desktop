// <copyright company="SIX Networks GmbH" file="SixRepoApp.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Play.Core.Games.Legacy.Repo
{
    public enum AppType
    {
        Teamspeak3,
        Mumble
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Sync.Core.Models.Repositories.SixSync")]
    public class SixRepoApp : IBaseYaml, IHaveType<AppType>
    {
        public SixRepoApp() {}

        [DataMember]
        public string Channel { get; set; }
        [DataMember]
        public string ChannelPassword { get; set; }
        [DataMember]
        public string Name { get; set; }
        public string Address
        {
            get
            {
                switch (Type) {
                case AppType.Teamspeak3: {
                    // #ts3server://ts3.hoster.com?port=9987&nickname=UserNickname&password=serverPassword&channel=MyDefaultChannel&channelpassword=defaultChannelPassword&token=TokenKey&addbookmark=1
                    var baseUrl = String.Format("ts3server://{0}?port={1}", Ip, Port > 0 ? Port : 9898);
                    if (!string.IsNullOrWhiteSpace(Password))
                        baseUrl += "&password=" + Password;
                    if (!string.IsNullOrWhiteSpace(Channel))
                        baseUrl += "&channel=" + Channel;
                    if (!string.IsNullOrWhiteSpace(ChannelPassword))
                        baseUrl += "&channelpassword=" + ChannelPassword;
                    return baseUrl;
                }
                default: {
                    return null;
                }
                }
            }
        }
        [DataMember]
        public bool IsHidden { get; set; }
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public AppType Type { get; set; }

        public string GetDisplayName() {
            return string.Format("{0}: {1}", Type, Name);
        }

        public void FromYaml(YamlMappingNode mapping) {
            foreach (var entry in mapping.Children) {
                var key = ((YamlScalarNode) entry.Key).Value;
                switch (key) {
                case ":type":
                    var type = YamlExtensions.GetStringOrDefault(entry.Value);
                    Type = string.IsNullOrWhiteSpace(type)
                        ? AppType.Teamspeak3
                        : (AppType) Enum.Parse(typeof (AppType), type);
                    break;
                case ":ip":
                    Ip = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":port":
                    Port = YamlExtensions.GetIntOrDefault(entry.Value);
                    break;
                case ":channel":
                    Channel = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":channel_password":
                    ChannelPassword = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":password":
                    Password = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":hidden":
                    IsHidden = YamlExtensions.GetBoolOrDefault(entry.Value);
                    break;
                }
            }
        }

        public string ToYaml() {
            throw new NotImplementedException();
        }
    }
}