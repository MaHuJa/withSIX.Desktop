// <copyright company="SIX Networks GmbH" file="FavoriteServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Net;
using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "FavoriteServer",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteServer : PropertyChangedBase, IServerBla
    {
        [DataMember] ServerAddress _address;
        [DataMember] string _gameName;
        [DataMember, Obsolete] IPAddress _ip;
        [DataMember, Obsolete] string _ipAddress;
        [DataMember] string _mod;
        [DataMember] string _name;
        [DataMember, Obsolete] int _port;
        [DataMember] ServerQueryMode _queryMode;

        public FavoriteServer(Server server) {
            _queryMode = server.QueryMode;
            _address = server.Address;
            _name = server.Name;
            _mod = String.Join(";", server.Mods); // Loss of info
            _gameName = server.GameName;
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

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext sc) {
            if (_address != null)
                return;
            var ip = _ip ?? TryParseIp(_ipAddress);
            if (ip == null || _port < 1 || _port > IPEndPoint.MaxPort)
                return;
            _address = new ServerAddress(ip, _port);
            _ip = null;
            _ipAddress = null;
        }

        static IPAddress TryParseIp(string ipAddress) {
            IPAddress ip;
            if (ipAddress != null && IPAddress.TryParse(ipAddress, out ip))
                return ip;
            return null;
        }

        public void Update(Server server) {
            _mod = String.Join(";", server.Mods);
            _name = server.Name;
            _gameName = server.GameName;
            _queryMode = server.QueryMode;
        }
    }

    public interface IServerBla
    {
        ServerQueryMode QueryMode { get; }
        ServerAddress Address { get; }
        string Name { get; }
        string GameName { get; }
        string Mod { get; }
    }
}