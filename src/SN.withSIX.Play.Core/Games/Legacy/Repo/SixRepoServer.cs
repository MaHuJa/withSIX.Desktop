// <copyright company="SIX Networks GmbH" file="SixRepoServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Play.Core.Games.Legacy.Repo
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Sync.Core.Models.Repositories.SixSync"
        )]
    public class SixRepoServer : IBaseYaml
    {
        static readonly Dictionary<string, string> mapping = new Dictionary<string, string> {
            {"a3", GameUUids.Arma3},
            {"a2", GameUUids.Arma2Co},
            {"a2co", GameUUids.Arma2Co},
            {"arma3", GameUUids.Arma3},
            {"arma2oaco", GameUUids.Arma2Co},
            {"arma2", GameUUids.Arma2},
            {"arma2co", GameUUids.Arma2Co},
            {"arma2oa", GameUUids.Arma2Oa},
            {"toh", GameUUids.TKOH}
        };
        public static readonly string[] SYS = {":modlist", ":sixmodlist"};
        ServerAddress _address;
        string[] _allowedMods;
        string _ip;
        int _port;
        string[] _requiredMods;

        public SixRepoServer() {
            Motd = new string[0];
            Rules = new string[0];
            RequiredMods = new string[0];
            AllowedMods = new string[0];
            Apps = new string[0];
            Missions = new string[0];
            MPMissions = new string[0];
        }

        public ServerAddress Address
        {
            get { return _address ?? (_address = GetAddy()); }
            private set { _address = value; }
        }

        public ServerAddress GetQueryAddress() {
            var a = Address;
            if (a == null)
                return null;
            return Address.ToQueryAddress();
        }
        [DataMember]
        public string Game { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string ImageLarge { get; set; }
        public Server Server { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Uuid { get; set; }
        [DataMember]
        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                Address = null;
            }
        }
        [DataMember]
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                Address = null;
            }
        }
        [DataMember]
        public string Info { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string[] Motd { get; set; }
        [DataMember]
        public string[] Rules { get; set; }
        [DataMember]
        public string[] RequiredMods
        {
            get { return _requiredMods; }
            set
            {
                Validate(value);
                _requiredMods = value;
            }
        }
        [DataMember]
        public string[] AllowedMods
        {
            get { return _allowedMods; }
            set
            {
                Validate(value);
                _allowedMods = value;
            }
        }
        [DataMember]
        public string[] Apps { get; set; }
        [DataMember]
        public string[] Missions { get; set; }
        [DataMember]
        public string[] MPMissions { get; set; }
        [DataMember]
        public bool IsOpen { get; set; }
        [DataMember]
        public bool IsHidden { get; set; }
        [DataMember]
        public bool ForceModUpdate { get; set; }
        [DataMember]
        public bool ForceServerName { get; set; }

        public string ToYaml() {
            var graph = new Dictionary<string, object> {
                {":name", Name},
                {":ip", Ip},
                {":port", Port},
                {":uuid", Uuid},
                {":game", Game},
                {":force_server_name", ForceServerName},
                {":open", IsOpen},
                {":hidden", IsHidden},
                {":force_mod_update", ForceModUpdate},
                {":info", Info},
                {":motd", Motd},
                {":password", Password},
                {":rules", Rules},
                {":image", Image},
                {":image_large", ImageLarge},
                {":required_mods", RequiredMods},
                {":allowed_mods", AllowedMods},
                {":apps", Apps},
                {":missions", Missions},
                {":mpmissions", MPMissions}
            };
            return graph._ToYaml();
        }

        public void FromYaml(YamlMappingNode mapping) {
            foreach (var entry in mapping.Children) {
                var key = ((YamlScalarNode) entry.Key).Value;
                switch (key) {
                case ":name":
                    Name = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":uuid":
                    Uuid = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":game":
                    Game = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":ip":
                    Ip = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":port":
                    Port = YamlExtensions.GetIntOrDefault(entry.Value);
                    break;
                case ":open":
                    IsOpen = YamlExtensions.GetIntOrDefault(entry.Value) >= 1
                             || YamlExtensions.GetBoolOrDefault(entry.Value);
                    break;
                case ":force_server_name":
                    ForceServerName = YamlExtensions.GetBoolOrDefault(entry.Value);
                    break;
                case ":hidden":
                    IsHidden = YamlExtensions.GetBoolOrDefault(entry.Value);
                    break;
                case ":force_mod_update":
                    ForceModUpdate = YamlExtensions.GetBoolOrDefault(entry.Value);
                    break;
                case ":info":
                    Info = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":password":
                    Password = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":motd":
                    Motd = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":rules":
                    Rules = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":image":
                    Image = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":image_large":
                    ImageLarge = YamlExtensions.GetStringOrDefault(entry.Value);
                    break;
                case ":required_mods":
                    RequiredMods = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":allowed_mods":
                    AllowedMods = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":missions":
                    Missions = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":mpmissions":
                    MPMissions = YamlExtensions.GetStringArray(entry.Value);
                    break;
                case ":apps":
                    Apps = YamlExtensions.GetStringArray(entry.Value);
                    break;
                }
            }
        }

        public Guid GetGameUuid() {
            var game = Game;
            if (string.IsNullOrWhiteSpace(game))
                return Guid.Empty;

            var key = game.ToLower();
            return new Guid(mapping.ContainsKey(key) ? mapping[key] : game);
        }

        public string PrettyPrint() {
            throw new NotImplementedException();
        }

        ServerAddress GetAddy() {
            return SAStuff.GetAddy(String.Format("{0}:{1}", _ip, _port == 0 ? 2302 : _port));
        }

        static void Validate(IEnumerable<string> concat) {
            if (concat == null)
                return;

            foreach (var i in concat.Where(x => !SYS.Contains(x)))
                withSIX.Core.Validators.PathValidator.ValidateName(i);
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context) {
            if (Motd == null)
                Motd = new string[0];
            if (Rules == null)
                Rules = new string[0];
            if (RequiredMods == null)
                RequiredMods = new string[0];
            if (AllowedMods == null)
                AllowedMods = new string[0];
            if (Apps == null)
                Apps = new string[0];
            if (Missions == null)
                Missions = new string[0];
            if (MPMissions == null)
                MPMissions = new string[0];
        }
    }
}