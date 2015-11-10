// <copyright company="SIX Networks GmbH" file="ArmaServerFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using ReactiveUI;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Options.Filters
{
    [DataContract(Name = "ServerFilter", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Filters")]
    public class ArmaServerFilter : FilterBase<Server>, IHaveModdingFilters
    {
        static readonly string[] _difficulties = {"Recruit", "Regular", "Veteran", "Mercenary"};
        static readonly string[] _protections = {"NONE", "BAD", "GOOD", "FULL"};
        static readonly string[] _gameModes = {" ", "COOP", "CTF", "CTI", "DM"};
        static readonly Dictionary<string, string[]> _GameModeDic = new Dictionary<string, string[]> {
            {"COOP", new[] {"COOP", "DOM"}}
        };
        static readonly string[] _gameStates = {"Creating", "Briefing", "Playing", "Debriefing", "Waiting"};
        static readonly string[] _continents = {
            " ", "Asia", "Europe", "Oceania",
            "North America", "South America"
        };
        [DataMember] string _Continent;
        IList<string> _Continents = new List<string>(_continents);
        [DataMember] ReactiveList<string> _Countries = new ReactiveList<string>();
        IList<string> _CountriesList = new List<string>(CountryFlagsMapping.CountryDict.Keys);
        IList<string> _Difficulties = new List<string>(_difficulties);
        [DataMember] int? _Difficulty = -1;
        [DataMember] string _GameMode;
        IList<string> _GameModes = new List<string>(_gameModes);
        [DataMember] int? _GameState = -1;
        IList<string> _GameStates = new List<string>(_gameStates);
        [DataMember] bool _HideEmpty;
        [DataMember] bool _HideFull;
        [DataMember] bool _HideNeverJoined;
        [DataMember] bool _HidePasswordProtected = true;
        [DataMember] bool _HideUnofficial;
        [DataMember] bool _HideUnresponsive;
        [DataMember] bool _HideWrongGameVersion = true;
        [DataMember] bool _IncompatibleServers = true;
        [DataMember] string _Island;
        [DataMember] long? _MaxPing = 0;
        [DataMember] long? _MaxPlayers = 0;
        [DataMember] long? _MinPlayers = 0;
        [DataMember] string _Mission;
        [DataMember] string _Mod;
        [DataMember] bool _Modded = true;
        [DataMember] string _Name;
        [DataMember] string _Player;
        [DataMember] int? _Protection = -1;
        IList<string> _Protections = new List<string>(_protections);
        protected bool? FilterMod = true;
        public string Mod
        {
            get { return _Mod; }
            set
            {
                if (SetProperty(ref _Mod, value))
                    PublishFilter();
            }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                if (SetProperty(ref _Name, value))
                    PublishFilter();
            }
        }
        public string Mission
        {
            get { return _Mission; }
            set
            {
                if (SetProperty(ref _Mission, value))
                    PublishFilter();
            }
        }
        public string Island
        {
            get { return _Island; }
            set
            {
                if (SetProperty(ref _Island, value))
                    PublishFilter();
            }
        }
        public string Player
        {
            get { return _Player; }
            set
            {
                if (SetProperty(ref _Player, value))
                    PublishFilter();
            }
        }
        public IList<string> Difficulties
        {
            get { return _Difficulties ?? (_Difficulties = new List<string>(_difficulties)); }
            set { SetProperty(ref _Difficulties, value); }
        }
        public IList<string> Protections
        {
            get { return _Protections ?? (_Protections = new List<string>(_protections)); }
            set { SetProperty(ref _Protections, value); }
        }
        public string GameMode
        {
            get { return _GameMode; }
            set
            {
                if (SetProperty(ref _GameMode, value))
                    PublishFilter();
            }
        }
        public int GameState
        {
            get { return _GameState ?? (int) (_GameState = -1); }
            set
            {
                if (SetProperty(ref _GameState, value))
                    PublishFilter();
            }
        }
        public string Continent
        {
            get { return _Continent; }
            set
            {
                if (SetProperty(ref _Continent, value))
                    PublishFilter();
            }
        }
        public IList<string> GameModes
        {
            get { return _GameModes ?? (_GameModes = new List<string>(_gameModes)); }
            set
            {
                if (SetProperty(ref _GameModes, value))
                    PublishFilter();
            }
        }
        public IList<string> GameStates
        {
            get { return _GameStates ?? (_GameStates = new List<string>(_gameStates)); }
            set { SetProperty(ref _GameStates, value); }
        }
        public IList<string> Continents
        {
            get { return _Continents ?? (_Continents = new List<string>(_continents)); }

            set { SetProperty(ref _Continents, value); }
        }
        public IList<string> CountriesList
        {
            get { return _CountriesList ?? (_CountriesList = new List<string>(CountryFlagsMapping.CountryDict.Keys)); }
            set { SetProperty(ref _CountriesList, value); }
        }
        public ReactiveList<string> Countries
        {
            get { return _Countries ?? (_Countries = new ReactiveList<string>()); }
            set
            {
                if (SetProperty(ref _Countries, value))
                    PublishFilter();
            }
        }
        public int Difficulty
        {
            get { return _Difficulty ?? (int) (_Difficulty = -1); }
            set
            {
                if (SetProperty(ref _Difficulty, value))
                    PublishFilter();
            }
        }
        public int Protection
        {
            get { return _Protection ?? (int) (_Protection = -1); }
            set
            {
                if (SetProperty(ref _Protection, value))
                    PublishFilter();
            }
        }
        public long MaxPing
        {
            get { return _MaxPing ?? (long) (_MaxPing = 0); }
            set
            {
                if (SetProperty(ref _MaxPing, value))
                    PublishFilter();
            }
        }
        public long MinPlayers
        {
            get { return _MinPlayers ?? (long) (_MinPlayers = 0); }
            set
            {
                if (SetProperty(ref _MinPlayers, value))
                    PublishFilter();
            }
        }
        public long MaxPlayers
        {
            get { return _MaxPlayers ?? (long) (_MaxPlayers = 0); }
            set
            {
                if (SetProperty(ref _MaxPlayers, value))
                    PublishFilter();
            }
        }
        public bool HideEmpty
        {
            get { return _HideEmpty; }
            set
            {
                if (SetProperty(ref _HideEmpty, value))
                    PublishFilter();
            }
        }
        public bool HideFull
        {
            get { return _HideFull; }
            set
            {
                if (SetProperty(ref _HideFull, value))
                    PublishFilter();
            }
        }
        public bool HideUnofficial
        {
            get { return Common.Flags.LockDown || _HideUnofficial; }
            set
            {
                if (SetProperty(ref _HideUnofficial, value))
                    PublishFilter();
            }
        }
        public bool HideNeverJoined
        {
            get { return _HideNeverJoined; }
            set
            {
                if (SetProperty(ref _HideNeverJoined, value))
                    PublishFilter();
            }
        }
        /*
        [DataMember]
        private bool _HideIncompatible = true;
        public bool HideIncompatible
        {
            get { return _HideIncompatible; }
            set { if (SetProperty(ref _HideIncompatible, value)) PublishFilter(); }
        }
        */

        public bool HidePasswordProtected
        {
            get { return _HidePasswordProtected; }
            set
            {
                if (SetProperty(ref _HidePasswordProtected, value))
                    PublishFilter();
            }
        }
        public bool HideWrongGameVersion
        {
            get { return _HideWrongGameVersion; }
            set
            {
                if (SetProperty(ref _HideWrongGameVersion, value))
                    PublishFilter();
            }
        }
        public bool HideUnresponsive
        {
            get { return _HideUnresponsive; }
            set
            {
                if (SetProperty(ref _HideUnresponsive, value))
                    PublishFilter();
            }
        }
        public bool Modded
        {
            get { return _Modded; }
            set
            {
                if (SetProperty(ref _Modded, value))
                    PublishFilter();
            }
        }
        public bool IncompatibleServers
        {
            get { return _IncompatibleServers; }
            set
            {
                if (SetProperty(ref _IncompatibleServers, value))
                    PublishFilter();
            }
        }

        protected override void ClearFilters() {
            _supressPublish = true;

            Name = null;
            Player = null;
            MinPlayers = 0;
            MaxPlayers = 0;
            MaxPing = 0;
            Mod = null;

            Continent = null;
            if (Countries == null) {
                Countries = new ReactiveList<string>();
                Countries.CollectionChanged += Countries_CollectionChanged;
            } else
                Countries.Clear();

            Protection = -1;

            IncompatibleServers = false;
            Modded = false;
            HideEmpty = false;
            HideFull = false;
            HideUnofficial = false;
            HideNeverJoined = false;
            HidePasswordProtected = false;
            HideUnresponsive = false;
            HideWrongGameVersion = false;

            DefaultMissionFilters();

            _supressPublish = false;

            base.DefaultFilters();
        }

        //[DataMember] private bool? _Favorite;
        public override void DefaultFilters() {
            _supressPublish = true;

            Name = null;
            Player = null;
            MinPlayers = 0;
            MaxPlayers = 0;
            MaxPing = 0;
            Mod = null;

            Continent = null;
            if (Countries == null) {
                Countries = new ReactiveList<string>();
                Countries.CollectionChanged += Countries_CollectionChanged;
            } else
                Countries.Clear();

            Protection = -1;

            IncompatibleServers = true;
            Modded = true;
            HideEmpty = false;
            HideFull = false;
            HideUnofficial = false;
            HideNeverJoined = false;
            HidePasswordProtected = true;
            HideUnresponsive = false;
            HideWrongGameVersion = true;

            DefaultMissionFilters();

            _supressPublish = false;

            base.DefaultFilters();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (Countries == null)
                Countries = new ReactiveList<string>();

            Countries.CollectionChanged += Countries_CollectionChanged;
        }

        [ReportUsage("ServerFilter: PublishFilter")]
        public override void PublishFilter() {
            base.PublishFilter();
        }

        public override bool AnyFilterEnabled() {
            if (HideEmpty)
                return true;
            if (HideFull)
                return true;
            if (HideUnofficial)
                return true;
            if (HideNeverJoined)
                return true;
            if (HidePasswordProtected)
                return true;
            if (HideUnresponsive)
                return true;
            if (HideWrongGameVersion)
                return true;
            if (IncompatibleServers)
                return true;
            if (Modded)
                return true;
            if (Countries.Count > 0)
                return true;
            if (MinPlayers != 0)
                return true;
            if (MaxPlayers != 0)
                return true;
            if (MaxPing != 0)
                return true;
            if (Protection != -1)
                return true;
            if (Difficulty != -1)
                return true;
            if (GameState > 0)
                return true;
            if (!string.IsNullOrWhiteSpace(Name))
                return true;
            if (!String.IsNullOrWhiteSpace(GameMode))
                return true;
            if (!string.IsNullOrWhiteSpace(Island))
                return true;
            if (!string.IsNullOrWhiteSpace(Player))
                return true;
            if (!string.IsNullOrWhiteSpace(Mission))
                return true;
            if (!string.IsNullOrWhiteSpace(Mod))
                return true;

            return false;
        }

        protected void DefaultMissionFilters() {
            Mission = null;
            Island = null;
            GameState = -1;
            GameMode = null;
            Difficulty = -1;
        }

        void Countries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!_supressPublish)
                PublishFilter();
        }

        protected override void ExecutePublish() {
            Filtered = AnyFilterEnabled();
            if (_save)
                DomainEvilGlobal.Settings.RaiseChanged();
            _save = true;

            base.ExecutePublish();
        }

        bool ModFilter(Server server, Collection collection) {
            /*            var customModSet = collection as CustomCollection;
            if (customModSet != null && customModSet.CustomRepo != null)
                return server.IsOfficial;*/

            return (collection == null && !Modded)
                   || server.HasMod(collection, Modded, IncompatibleServers);
        }

        public override bool Handler(Server server) {
            var game = DomainEvilGlobal.SelectedGame.ActiveGame;
            var modSet = game.CalculatedSettings.Collection;
            if (FilterMod.GetValueOrDefault(true) && !ModFilter(server, modSet))
                return false;

            if (!server.IsFavorite) {
                if (MaxPing != 0 &&
                    (server.Ping != null && (server.Ping == Common.MagicPingValue || server.Ping > MaxPing)))
                    return false;

                if (!server.HasBasicInfo)
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(Name)) {
                if (!AdvancedStringSearch(Name, server.Name))
                    return false;
            }

            if (HideUnofficial && !server.IsOfficial)
                return false;

            if (HideUnresponsive && (server.Ping == null || server.Ping == Common.MagicPingValue))
                return false;

            if (HideEmpty && server.NumPlayers == 0)
                return false;

            if (HideNeverJoined && server.LastJoinedOn == null)
                return false;

            if (HideFull && server.FreeSlots == 0 && server.NumPlayers > 0)
                return false;

            if (HidePasswordProtected && server.PasswordRequired)
                return false;

            if (MinPlayers != 0 && server.NumPlayers < MinPlayers)
                return false;

            if (MaxPlayers != 0 && server.NumPlayers > MaxPlayers)
                return false;

            if (Protection != -1 && Protection > 0 && (int) server.Protection != Protection - 1)
                return false;

            if (Difficulty != -1 && Difficulty > 0 && server.Difficulty != Difficulty - 1)
                return false;

            /*
             * States:
            "Unknown",
            "Waiting",
            "Unknown",
            "Creating",
            "Unknown",
            "Loading",
            "Briefing",
            "Playing",
            "Unknown",
            "Debriefing",
             * 
             * vs
             * 
             * "Assignment", "Briefing", "Playing", "Debriefing"
             */

            if (GameState > 0) {
                var gs = GameState - 1;
                if (gs == 0 && (server.GameState != 2))
                    return false;
                if (gs == 1 && (server.GameState != 5 && server.GameState != 6))
                    return false;
                if (gs == 2 && (server.GameState != 7))
                    return false;
                if (gs == 3 && (server.GameState != 9))
                    return false;
                if (gs == 4 && (server.GameState != 1))
                    return false;
            }

            if (!String.IsNullOrWhiteSpace(GameMode)) {
                if (server.GameType == null)
                    return false;

                if (_GameModeDic.ContainsKey(GameMode)) {
                    if (_GameModeDic[GameMode].None(
                        x => server.GameType.NullSafeContainsIgnoreCase(x)))
                        return false;
                } else {
                    if (!server.GameType.NullSafeContainsIgnoreCase(GameMode))
                        return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(Island)) {
                if (!AdvancedStringSearch(Island, server.Island))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(Player)) {
                if (!AdvancedStringSearch(Player, "",
                    normalTestFunc: search => {
                        if (server.Players == null || server.Players.None(
                            x => x.Name.NullSafeContainsIgnoreCase(search)))
                            return false;
                        return true;
                    },
                    reverseTestFunc: search => {
                        if (server.Players != null && server.Players.Any(
                            x => x.Name.NullSafeContainsIgnoreCase(search)))
                            return true;
                        return false;
                    }))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(Mission)) {
                if (!AdvancedStringSearch(Mission, server.Mission))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(Mod)) {
                if (!AdvancedStringSearch(Mod, "",
                    normalTestFunc: search => {
                        if (server.Mods == null || server.Mods.None(x => x.ContainsIgnoreCase(search)))
                            return false;
                        return true;
                    },
                    reverseTestFunc: search => {
                        if (server.Mods.Any(x => x.ContainsIgnoreCase(search)))
                            return true;
                        return false;
                    }))
                    return false;
            }

            if (Countries.Count > 0 && Countries.None(x => server.Country == x))
                return false;

            if (HideWrongGameVersion &&
                !server.GetIsSameGameVersion(server.ReqBuild,
                    game.InstalledState.Version,
                    server.GameVer))
                return false;

            return true;
        }
    }

    public class CleanServerFilter : ArmaServerFilter
    {
        public CleanServerFilter() {
            ClearFilters();
            FilterMod = false;
        }
    }
}