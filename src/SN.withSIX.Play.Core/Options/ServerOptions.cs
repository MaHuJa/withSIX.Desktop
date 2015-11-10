// <copyright company="SIX Networks GmbH" file="ServerOptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SN.withSIX.Api.Models;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "ServerOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class ServerOptions : OptionBase
    {
        [DataMember] bool _AutoProcessServerApps;
        [DataMember] List<FavoriteServer> _favorites = new List<FavoriteServer>();
        [DataMember] int? _MinFreeSlots = 5;
        [DataMember] int? _MinNumPlayers = 5;
        [DataMember] ConcurrentDictionary<string, string> _Passwords = new ConcurrentDictionary<string, string>();
        [DataMember] bool? _quickPlayApplyServerFilters = true;
        [DataMember] List<RecentServer> _recentServers = new List<RecentServer>();
        [DataMember] bool _ShowPingAsNumber;
        [DataMember] ViewType _viewType;
        public ViewType ViewType
        {
            get { return _viewType; }
            set { _viewType = value; }
        }
        public bool AutoProcessServerApps
        {
            get { return _AutoProcessServerApps; }
            set
            {
                if (SetProperty(ref _AutoProcessServerApps, value))
                    SaveSettings();
            }
        }
        ConcurrentDictionary<string, string> Passwords
        {
            get { return _Passwords ?? (_Passwords = new ConcurrentDictionary<string, string>()); }
        }
        public bool ShowPingAsNumber
        {
            get { return _ShowPingAsNumber; }
            set
            {
                if (_ShowPingAsNumber != value) {
                    _ShowPingAsNumber = value;
                    SaveSettings();
                }
            }
        }
        public int MinNumPlayers
        {
            get { return _MinNumPlayers.GetValueOrDefault(5); }
            set
            {
                if (_MinNumPlayers != value) {
                    _MinNumPlayers = value;
                    SaveSettings();
                }
            }
        }
        public int MinFreeSlots
        {
            get { return _MinFreeSlots.GetValueOrDefault(5); }
            set
            {
                if (_MinFreeSlots != value) {
                    _MinFreeSlots = value;
                    SaveSettings();
                }
            }
        }
        public List<FavoriteServer> Favorites
        {
            get { return _favorites ?? (_favorites = new List<FavoriteServer>()); }
            set { _favorites = value; }
        }
        public List<RecentServer> Recent
        {
            get { return _recentServers ?? (_recentServers = new List<RecentServer>()); }
            set { _recentServers = value; }
        }
        public bool QuickPlayApplyServerFilters
        {
            get { return _quickPlayApplyServerFilters.GetValueOrDefault(true); }
            set
            {
                if (_quickPlayApplyServerFilters != value) {
                    _quickPlayApplyServerFilters = value;
                    SaveSettings();
                }
            }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            Favorites.RemoveAll(x => x.Address == null);
            Recent.RemoveAll(x => x.Address == null);
        }

        protected string GetPassword(ServerAddress identifier) {
            string val;
            return Passwords.TryGetValue(identifier.ToString(), out val) ? val.Decode64() : null;
        }

        public string GetPassword(Server server) {
            return GetPassword(server.Address);
        }

        protected void SetPassword(ServerAddress identifier, string value) {
            if (value == null) {
                string val;
                Passwords.TryRemove(identifier.ToString(), out val);
                return;
            }
            var procced = value.EncodeTo64();
            Passwords.AddOrUpdate(identifier.ToString(), procced, (s, s1) => procced);
        }

        public void SetPassword(Server server, string value) {
            SetPassword(server.Address, value);
        }

        public bool IsFavorite(Server server) {
            return Favorites.Any(f => f.Matches(server));
        }

        public FavoriteServer UpdateFavorite(Server server) {
            var fav = Favorites.FirstOrDefault(f => f.Matches(server));
            if (fav != null) {
                fav.Update(server);
                return fav;
            }

            return null;
        }

        public void AddFavorite(Server server) {
            if (UpdateFavorite(server) != null)
                return;

            Favorites.Add(new FavoriteServer(server));
            SaveSettings();
        }

        public void RemoveFavorite(Server server) {
            var favorite = Favorites.FirstOrDefault(f => f.Matches(server));
            if (favorite == null)
                return;
            Favorites.Remove(favorite);
            SaveSettings();
        }

        public void AddRecent(Server server) {
            var recentServer = new RecentServer(server);
            Recent.Add(recentServer);
            server.LastJoinedOn = null;
            SaveSettings();
        }

        public void RemoveRecent(Server server) {
            Recent.RemoveAll(x => x.Address.Equals(server.Address));
            server.LastJoinedOn = null;
            SaveSettings();
        }
    }
}