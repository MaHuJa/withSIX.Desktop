// <copyright company="SIX Networks GmbH" file="Server.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Legacy.Servers;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface ISearchScore
    {
        int SearchScore { get; set; }
    }

    public abstract class Server : PropertyChangedBase, IComparePK<Server>, IHaveNotes, ICopyProperties, IEnableLogging,
        ISearchScore, IToggleFavorite, IHierarchicalLibraryItem
    {
        const string ApiPath = "stats/servers";
        static readonly Regex serverTimeRegex = new Regex(@"((GmT|Utc)[\s]*(?<Offset>([+]|[-])[\s]?[\d]{1,2})?)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex serverVersionRegex = new Regex(@"((\d+)(\.\d+)(\.\d+)*(\.\d+)*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly string[] ignoredProperties = {
            "Friends", "HasFriends"
        };
        static readonly Regex rxPwsUri =
            new Regex(@"\s*(" + SixRepo.PwsProtocolRegex + @"[\w-\.]+/[\w-_%\./]+\.yml)\s*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxPwsCollectionUri =
            new Regex(@"\s*(" + SixRepo.PwsProtocolRegex + @"\?c=[\w-_]+)\s*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly ISupportServers _game;
        int _difficulty;
        string _gameName;
        int _gameState;
        string _gameType = "Unknown";
        Version _gameVer;
        bool _hasBasicInfo;
        bool _hasFriends;
        bool? _hasNotes;
        bool _isFavorite;
        bool _isFeatured;
        string _island;
        bool _isOfficial;
        bool _isUpdating;
        int _maxPlayers;
        string _mission;
        bool _modded;
        string[] _mods;
        Version _modVersion;
        string _name;
        int _numPlayers;
        bool _passwordRequired;
        long? _ping = Common.MagicPingValue;
        ProtectionLevel _protection;
        int? _reqBuild;
        string _savedPassword;
        bool _savePassword;
        ServerAddress _serverAddress;
        DateTime? _serverTime;
        string[] _signatures;
        int _svBattleye;
        DateTime? _synced;
        int _verifySignatures;

        public Server(ISupportServers game, ServerAddress address) {
            Contract.Requires<ArgumentNullException>(game != null);
            Contract.Requires<ArgumentNullException>(address != null);

            Children = new ReactiveList<IHierarchicalLibraryItem>();

            _game = game;
            Friends = new ReactiveList<Friend>();
            Players = new Player[0];
            _mods = new string[0];
            _signatures = new string[0];

            Address = address.FixArmaQueryAddress();
            SetServerAddress(address.GetArmaServerPort().Port);

            _isFavorite = DomainEvilGlobal.Settings.ServerOptions.IsFavorite(this);
            _savedPassword = DomainEvilGlobal.Settings.ServerOptions.GetPassword(this);
            if (_savedPassword != null)
                _savePassword = true;
            TryDetermineCountry();
        }

        public bool HasFriends
        {
            get { return _hasFriends; }
            set { SetProperty(ref _hasFriends, value); }
        }
        public bool SavePassword
        {
            get { return _savePassword; }
            set { SetProperty(ref _savePassword, value); }
        }
        public ServerAddress Address { get; }
        public ServerAddress ServerAddress
        {
            get { return _serverAddress; }
            set { SetProperty(ref _serverAddress, value); }
        }
        public ServerQueryMode QueryMode { get; set; }
        public bool IsDedicated { get; set; }
        public ServerPlatform ServerPlatform { get; set; }
        public int? RequiredVersion { get; set; }
        public int? Language { get; set; }
        public Coordinates Coordinates { get; set; }
        public bool HasBasicInfo
        {
            get { return _hasBasicInfo; }
            set { SetProperty(ref _hasBasicInfo, value); }
        }
        public string[] IgnoredProperties
        {
            get { return ignoredProperties; }
        }
        public int SearchScore
        {
            get { return _searchScore; }
            set { SetProperty(ref _searchScore, value); }
        }

        [DoNotObfuscate]
        public void ToggleFavorite() {
            IsFavorite = !IsFavorite;
        }

        public string Details() {
            var sb = new StringBuilder();

            sb.AppendLine(String.Format("{0} {1}", ServerAddress, Name));
            sb.AppendLine(String.Format("{0} {1} on {2}", GameState, Mission, Island));
            sb.AppendLine(String.Format("{0}/{1} players", NumPlayers, MaxPlayers));

            return sb.ToString();
        }

        public async Task<ServerQueryState> UpdateAsync(IPEndPoint endPoint = null) {
            IsUpdating = true;
            try {
                var state = new ServerQueryState(endPoint) {Server = this};
                await _game.QueryServer(state).ConfigureAwait(false);
                return state;
            } finally {
                IsUpdating = false;
            }
        }

        public async Task TryUpdateAsync() {
            try {
                await UpdateAsync().ConfigureAwait(false);
            } catch (SocketException ex) {
                MainLog.Logger.Warn("Failed to reach {0} for server query. [{1}] {2}", Address, ex.GetType(), ex.Message);
            }
        }

        public Uri GetPwsUriFromName() {
            if (string.IsNullOrWhiteSpace(Name))
                return null;

            Uri uri;
            var match = rxPwsUri.Match(Name);
            if (!match.Success)
                return null;
            return Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out uri) ? uri : null;
        }

        public Uri GetPwsCollectionUriFromName() {
            if (string.IsNullOrWhiteSpace(Name))
                return null;

            Uri uri;
            var match = rxPwsCollectionUri.Match(Name);
            if (!match.Success)
                return null;
            return Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out uri) ? uri : null;
        }

        public void SetServerAddress(int port) {
            ServerAddress = new ServerAddress(Address.IP, port);
        }

        static Version ReadModVersion(string name) {
            if (name == null)
                return null;

            Version mv = null;
            var match = serverVersionRegex.Matches(name);
            if (match.Count > 0)
                mv = match[0].Value.ToNormalizedVersion();

            return mv;
        }

        public static Server FromStored(ISupportServers game, IServerBla stored) {
            return CreateServerFromStored(game, stored, ToDict(stored));
        }

        static Dictionary<string, string> ToDict(IServerBla stored) {
            return stored.QueryMode == ServerQueryMode.Steam
                ? new Dictionary<string, string> {
                    {"name", stored.Name},
                    {"modNames:1-1", stored.Mod},
                    {"folder", stored.GameName}
                }
                : new Dictionary<string, string> {
                    {"hostname", stored.Name},
                    {"mod", stored.Mod},
                    {"gamename", stored.GameName}
                };
        }

        static Server CreateServerFromStored(ISupportServers game, IServerBla stored,
            IDictionary<string, string> settings) {
            var server = game.CreateServer(stored.Address);
            server.UpdateInfoFromResult(CreateQueryResult(stored.QueryMode, settings));
            return server;
        }

        static ServerQueryResult CreateQueryResult(ServerQueryMode queryMode, IDictionary<string, string> settings) {
            switch (queryMode) {
            case ServerQueryMode.Gamespy:
                return new GamespyServerQueryResult(settings);
            case ServerQueryMode.Steam:
                return new SourceServerQueryResult(settings);
            case ServerQueryMode.All:
                return new SourceServerQueryResult(settings);
            default:
                throw new InvalidOperationException("Unsupported mode: " + queryMode);
            }
        }

        #region IServer Members

        Player[] _players;
        int _searchScore;

        public bool IsOfficial
        {
            get { return _isOfficial; }
            set { SetProperty(ref _isOfficial, value); }
        }

        public bool IsFeatured
        {
            get { return _isFeatured; }
            set { SetProperty(ref _isFeatured, value); }
        }

        public DateTime? Synced
        {
            get { return _synced; }
            set { SetProperty(ref _synced, value); }
        }

        public int? ReqBuild
        {
            get { return _reqBuild; }
            set { SetProperty(ref _reqBuild, value); }
        }

        public string Country { get; private set; }

        public string[] Mods
        {
            get { return _mods; }
            protected set { SetProperty(ref _mods, value); }
        }

        public string[] Signatures
        {
            get { return _signatures; }
            set { SetProperty(ref _signatures, value); }
        }

        public Player[] Players
        {
            get { return _players; }
            set { SetProperty(ref _players, value); }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (!SetProperty(ref _name, value))
                    return;
                ModVersion = ReadModVersion(value);
                ServerTime = GetServerTime(value);
            }
        }

        public string SavedPassword
        {
            get { return _savedPassword; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                if (!SetProperty(ref _savedPassword, value))
                    return;
                if (SavePassword)
                    DomainEvilGlobal.Settings.ServerOptions.SetPassword(this, value);
            }
        }

        public bool PasswordRequired
        {
            get { return _passwordRequired; }
            set { SetProperty(ref _passwordRequired, value); }
        }

        public int NumPlayers
        {
            get { return _numPlayers; }
            set { SetProperty(ref _numPlayers, value); }
        }

        public int MaxPlayers
        {
            get { return _maxPlayers; }
            set { SetProperty(ref _maxPlayers, value); }
        }

        public int FreeSlots
        {
            get { return (MaxPlayers - NumPlayers); }
        }

        public string Mission
        {
            get { return _mission; }
            set { SetProperty(ref _mission, value); }
        }

        public string Island
        {
            get { return _island; }
            set { SetProperty(ref _island, value); }
        }

        public int Difficulty
        {
            get { return _difficulty; }
            set { SetProperty(ref _difficulty, value); }
        }

        public Version GameVer
        {
            get { return _gameVer; }
            set { SetProperty(ref _gameVer, value); }
        }

        public string GameType
        {
            get { return _gameType; }
            set { SetProperty(ref _gameType, value); }
        }

        public int GameState
        {
            get { return _gameState; }
            set { SetProperty(ref _gameState, value); }
        }

        public ProtectionLevel Protection
        {
            get { return _protection; }
            set { SetProperty(ref _protection, value); }
        }

        public int VerifySignatures
        {
            get { return _verifySignatures; }
            set
            {
                if (SetProperty(ref _verifySignatures, value))
                    Protection = GetProtection(value, SvBattleye);
            }
        }

        public int SvBattleye
        {
            get { return _svBattleye; }
            set
            {
                if (SetProperty(ref _svBattleye, value))
                    Protection = GetProtection(VerifySignatures, value);
            }
        }

        public long? Ping
        {
            get { return _ping; }
            set { SetProperty(ref _ping, value); }
        }

        public DateTime? ServerTime
        {
            get { return _serverTime; }
            set { SetProperty(ref _serverTime, value); }
        }

        public bool IsEmpty
        {
            get { return NumPlayers == 0; }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.ServerOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.ServerOptions.RemoveFavorite(this);
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        public ReactiveList<Friend> Friends { get; }

        public bool? IsNight
        {
            get
            {
                var serverTime = ServerTime;
                if (serverTime == null)
                    return null;

                return serverTime.Value.Hour < 5 || serverTime.Value.Hour > 19;
            }
        }

        public Version ModVersion
        {
            get { return _modVersion; }
            set { SetProperty(ref _modVersion, value); }
        }

        public bool IsUpdating
        {
            get { return _isUpdating; }
            set { SetProperty(ref _isUpdating, value); }
        }

        public DateTime? LastJoinedOn
        {
            get
            {
                var recent = DomainEvilGlobal.Settings.ServerOptions.Recent
                    .OrderByDescending(x => x.On)
                    .FirstOrDefault(x => x.Address.Equals(Address));

                if (recent == null)
                    return null;
                return recent.On;
            }
            set
            {
                OnPropertyChanged();
                OnPropertyChanged("LastJoinedOnAgo");
            }
        }

        public string LastJoinedOnAgo
        {
            get { return LastJoinedOn.HasValue ? LastJoinedOn.Value.Ago() : "Never"; }
        }

        public bool Modded
        {
            get { return _modded; }
            set { SetProperty(ref _modded, value); }
        }

        public string GameName
        {
            get { return _gameName; }
            set { SetProperty(ref _gameName, value); }
        }

        public bool ForceServerName { get; set; }

        public virtual bool ComparePK(object obj) {
            var emp = obj as Server;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        public bool ComparePK(Server other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            if (other.Address == null || Address == null)
                return false;
            return other.Address.Equals(Address);
        }

        public string Notes
        {
            get { return DomainEvilGlobal.NoteStorage.GetNotes(this); }
            set
            {
                DomainEvilGlobal.NoteStorage.SetNotes(this, value);
                _hasNotes = !string.IsNullOrEmpty(value);
                new[] {"Notes", "HasNotes"}.ForEach(OnPropertyChanged);
            }
        }

        public bool HasNotes
        {
            get
            {
                if (_hasNotes != null)
                    return (bool) _hasNotes;
                _hasNotes = DomainEvilGlobal.NoteStorage.HasNotes(this);
                return (bool) _hasNotes;
            }
        }

        public string NoteName
        {
            get { return Address.ToString().Replace(":", "_").Replace(".", "_"); }
        }

        public bool IsSameGameVersion(Version gameVer) {
            return GetIsSameGameVersion(ReqBuild, gameVer, GameVer);
        }

        public bool GetIsSameGameVersion(int? reqBuild, Version gameVer, Version serverVer) {
            if (reqBuild == null || reqBuild <= 0) {
                return gameVer != null
                       && serverVer != null
                       && serverVer.Major == gameVer.Major
                       && serverVer.Minor == gameVer.Minor;
            }

            return gameVer != null
                   && serverVer != null
                   && serverVer.Major == gameVer.Major
                   && serverVer.Minor == gameVer.Minor
                   && gameVer.Revision >= reqBuild;
        }

        public void AddFriend(Friend friend) {
            Friends.AddWhenMissing(friend);
            friend.Server = this;
            HasFriends = true;
        }

        public void RemoveFriend(Friend friend) {
            Friends.Remove(friend);
            HasFriends = Friends.Any();
            friend.Server = null;
        }

        public void UpdatePing(long ping) {
            var newPing = ping <= 0 ? Common.MagicPingValue : ping;
            if (newPing != Common.MagicPingValue || Ping == null || Ping <= 0)
                Ping = newPing;
        }

        public void UpdateInfoFromResult(ServerQueryResult serverResult) {
            try {
                UpdateInfoFromSettings(serverResult);
            } finally {
                IsUpdating = false;
                Synced = Tools.Generic.GetCurrentUtcDateTime;
            }
        }

        public virtual void UpdateModInfo(string modInfo) {
            if (modInfo == null)
                return;
            UpdateModInfo(modInfo.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
        }

        public virtual void UpdateModInfo(string[] mods) {
            if (mods == null)
                return;
            if (!mods.SequenceEqual(Mods))
                Mods = mods;
        }

        public abstract bool HasMod(Collection ms, bool filterModded, bool filterIncompatible);

        static ProtectionLevel GetProtection(int sig, int be) {
            if (sig >= 2 && be == 1)
                return ProtectionLevel.Full;
            if (sig >= 2)
                return ProtectionLevel.Medium;

            return sig != 1 && be != 1 ? ProtectionLevel.None : ProtectionLevel.Low;
        }

        static DateTime? GetServerTime(string name) {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var match = serverTimeRegex.Match(name);
            if (!match.Success)
                return null;

            var offset = match.Groups["Offset"].Value.Replace(" ", String.Empty);
            var offsetInt = offset.TryInt();

            return Tools.Generic.GetCurrentUtcDateTime.AddHours(offsetInt);
        }

        void TryDetermineCountry() {
            try {
                DetermineCountry();
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
        }

        public void UpdateInfoFromSettings(ServerQueryResult result) {
            ServerMapper.Instance.Map(result, this);

            var game = (Game) _game;
            var modSet = game.CalculatedSettings.Collection;
            IsOfficial = GetIsOfficial(modSet);
            IsFeatured = GetIsFeatured(modSet);

            if (IsFavorite)
                DomainEvilGlobal.Settings.ServerOptions.UpdateFavorite(this);
        }

        void DetermineCountry() {
            Country = Tools.Geo.Value.GetCountryCode(Address.IP);
        }

        bool GetIsFeatured(Collection collection) {
            return collection != null && collection.IsFeaturedServer(this);
        }

        bool GetIsOfficial(Collection collection) {
            return collection != null && collection.IsOfficialServer(this);
        }

        #endregion

        #region IHierarchicalLibraryItem

        public ReactiveList<IHierarchicalLibraryItem> Children { get; }
        public ICollectionView ChildrenView { get; private set; }
        public IHierarchicalLibraryItem SelectedItem { get; set; }
        public ObservableCollection<object> SelectedItemsInternal { get; set; }

        public void ClearSelection() {
            SelectedItemsInternal.Clear();
        }

        object IHaveSelectedItem.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (IHierarchicalLibraryItem) value; }
        }
        public ICollectionView ItemsView { get; private set; }

        #endregion
    }

    public interface IToggleFavorite
    {
        void ToggleFavorite();
    }

    public enum ServerPlatform
    {
        Windows,
        Linux
    }

    public class Coordinates
    {
        public Coordinates(double longitude, double latitude) {
            Longitude = longitude;
            Latitude = latitude;
        }

        public double Longitude { get; private set; }
        public double Latitude { get; private set; }
    }


    public abstract class Server<TGame> : Server where TGame : Game, ISupportServers
    {
        protected readonly TGame Game;

        protected Server(TGame game, ServerAddress address) : base(game, address) {
            Game = game;
        }
    }
}