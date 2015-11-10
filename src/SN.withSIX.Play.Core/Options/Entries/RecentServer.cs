// <copyright company="SIX Networks GmbH" file="RecentServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "RecentServer",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class RecentServer : PropertyChangedBase, IServerBla
    {
        [DataMember] readonly DateTime _on;
        [DataMember] ServerAddress _address;
        [DataMember] string _gameName;
        [DataMember] string _mod;
        [DataMember] string _name;
        [DataMember] ServerQueryMode _queryMode;

        public RecentServer(Server server) {
            _queryMode = server.QueryMode;
            _address = server.Address;
            _mod = String.Join(";", server.Mods); // Loss of info
            _name = server.Name;
            _gameName = server.GameName;
            _on = Tools.Generic.GetCurrentUtcDateTime;
        }

        public DateTime On
        {
            get { return _on; }
        }
        public string Name
        {
            get { return _name; }
        }
        public string Mod
        {
            get { return _mod; }
        }
        public string GameName
        {
            get { return _gameName; }
        }
        public ServerAddress Address
        {
            get { return _address; }
        }
        public ServerQueryMode QueryMode
        {
            get { return _queryMode; }
        }

        public bool Matches(Server server) {
            return server != null && server.Address.Equals(Address);
        }
    }
}