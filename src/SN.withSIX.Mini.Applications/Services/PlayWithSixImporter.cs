// <copyright company="SIX Networks GmbH" file="PlayWithSixImporter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Applications.Services.Dtos;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SubscribedCollection = SN.withSIX.Mini.Applications.Services.Dtos.SubscribedCollection;

namespace SN.withSIX.Mini.Applications.Services
{
    public interface IPlayWithSixImporter
    {
        IAbsoluteFilePath DetectPwSSettings();
        Task ImportPwsSettings(IAbsoluteFilePath filePath);
        Task<bool> ShouldImport();
    }

    // TODO: Import local mods??
    public class PlayWithSixImporter : IPlayWithSixImporter, IApplicationService
    {
        public const int ImportVersion = 1;
        readonly IDbContextLocator _locator;

        public PlayWithSixImporter(IDbContextLocator locator) {
            _locator = locator;
        }

        public IAbsoluteFilePath DetectPwSSettings() {
            var pwsPath = PathConfiguration.GetRoamingRootPath().GetChildDirectoryWithName("Play withSIX");
            return !pwsPath.Exists ? null : GetLatestSettingsPath(pwsPath);
        }

        public async Task<bool> ShouldImport() {
            var settings = _locator.GetSettingsContext();
            await TaskExt.Default; // contract workaround :S
            return !settings.Settings.Local.DeclinedPlaywithSixImport &&
                   settings.Settings.Local.PlayWithSixImportVersion != ImportVersion;
        }

        public async Task ImportPwsSettings(IAbsoluteFilePath filePath) {
            try {
                await ImportPwsSettingsInternal(filePath).ConfigureAwait(false);
            } catch (Exception ex) {
                throw new ValidationException(
                    "A problem ocurred while trying to import settings from PwS. Please make sure the settings are of the latest PwS version",
                    ex);
            }
        }

        async Task ImportPwsSettingsInternal(IAbsoluteFilePath filePath) {
            var pwsSettings = filePath.LoadXml<UserSettings>();
            var db = _locator.GetGameContext();
            await db.LoadAll();
            foreach (var g in db.Games) {
                var ss = pwsSettings.GameOptions.GameSettingsController.Profiles.FirstOrDefault()?.GameSettings;
                if (ss != null && ss.ContainsKey(g.Id))
                    HandleGameSettings(pwsSettings, g);
                //HandleGameContent(pwsSettings, g);
            }

            await db.SaveChanges().ConfigureAwait(false);
            // TODO
            var settings = _locator.GetSettingsContext();
            settings.Settings.Local.PlayWithSixImportVersion = ImportVersion;
            await settings.SaveSettings().ConfigureAwait(false);
        }

        void HandleGameContent(UserSettings pwsSettings, Game game) {
            foreach (var c in pwsSettings.ModOptions.CustomModSets.Where(x => x.GameId == game.Id))
                ConvertToCollection(c, game);
            foreach (var c in pwsSettings.ModOptions.SubscribedModSets.Where(x => x.GameId == game.Id))
                ConvertToCollection(c, game);
            /*            foreach (var c in pwsSettings.ModOptions.Favorites) {
                            var existing = game.Collections.FirstOrDefault(x => x.Id == c.)
                        }*/
        }

        static void ConvertToCollection(SubscribedCollection p0, Game game) {
            var exists = game.SubscribedCollections.Any(x => x.Id == p0.CollectionID);
            if (exists)
                return;
            game.Contents.Add(new Mini.Core.Games.SubscribedCollection(p0.CollectionID, p0.Name, game.Id));
        }

        void ConvertToCollection(CustomCollection p0, Game game) {
            var isPublished = p0.PublishedId.HasValue;
            var exists = isPublished
                ? game.SubscribedCollections.Any(x => x.Id == p0.PublishedId.Value)
                : game.LocalCollections.Any(x => x.Id == p0.ID);
            if (exists)
                return;

            // TODO: We can only support Private Published Collections once we have a stable Bearer token to use...
            if (isPublished)
                game.Contents.Add(new Mini.Core.Games.SubscribedCollection(p0.PublishedId.Value, p0.Name, game.Id));
            else {
                // TODO: If published, the collection should become a SubscribedCollection? Perhaps with IsOwner flag??
                var modNames = p0.AdditionalMods.Concat(p0.OptionalMods).Concat(p0.Mods).Where(x => x != null)
                    .DistinctBy(x => x.ToLower());
                var packagedContents = game.Contents.OfType<IHavePackageName>();
                var contentDict = modNames.ToDictionary(x => x,
                    x =>
                        packagedContents.FirstOrDefault(
                            c => c.PackageName.Equals(x, StringComparison.CurrentCultureIgnoreCase)) ??
                        CreateLocal(game, x));
                game.Contents.Add(new LocalCollection(game.Id, p0.Name,
                    contentDict.Values.Select(x => new ContentSpec((Content) x)).ToList()));
            }
            // TODO: We should synchronize the network again before executing actions..
        }

        static ModLocalContent CreateLocal(Game game, string x) {
            var modLocalContent = new ModLocalContent(x, x.ToLower(), game.Id, null);
            game.Contents.Add(modLocalContent);
            return modLocalContent;
        }

        static void HandleGameSettings(UserSettings pwsSettings, Game g) {
            var gs = g.Settings as IHavePackageDirectory;
            var modDir = GetGameValue<string>(pwsSettings, g, "ModDirectory");
            if (gs != null && modDir != null)
                gs.PackageDirectory = modDir.ToAbsoluteDirectoryPath();
            var repoDir = GetGameValue<string>(pwsSettings, g, "RepositoryDirectory");
            if (repoDir != null)
                g.Settings.RepoDirectory = repoDir.ToAbsoluteDirectoryPath();
            var gameDir = GetGameValue<string>(pwsSettings, g, "Directory");
            if (gameDir != null)
                g.Settings.GameDirectory = gameDir.ToAbsoluteDirectoryPath();
            var startupLine = GetGameValue<string>(pwsSettings, g, "StartupLine");
            if (startupLine != null)
                g.Settings.StartupParameters.StartupLine = startupLine;
        }

        static T GetGameValue<T>(UserSettings pwsSettings, Game g, string propertyName) {
            return pwsSettings.GameOptions.GameSettingsController.GetValue<T>(g.Id, propertyName);
        }

        static IAbsoluteFilePath GetLatestSettingsPath(IAbsoluteDirectoryPath pwsPath) {
            var versions =
                GetParsedVersions(Directory.EnumerateFiles(pwsPath.ToString(), "settings-*.xml"));

            var compatibleVersion = versions.FirstOrDefault();
            return compatibleVersion == null ? null : GetVersionedSettingsPath(compatibleVersion, pwsPath);
        }

        static IOrderedEnumerable<Version> GetParsedVersions(IEnumerable<string> filePaths) {
            return
                filePaths.Select(x => ParseSettingsFileVersion(Path.GetFileNameWithoutExtension(x)))
                    .Where(x => x != null)
                    .OrderByDescending(x => x);
        }

        static Version ParseSettingsFileVersion(string settingsFileName) {
            return settingsFileName.Replace("settings-", "").TryParseVersion();
        }

        static IAbsoluteFilePath GetVersionedSettingsPath(Version version, IAbsoluteDirectoryPath pwsPath) {
            return
                pwsPath.GetChildFileWithName(String.Format("settings-{0}.{1}.xml", version.Major,
                    version.Minor));
        }
    }
}

namespace SN.withSIX.Mini.Applications.Services.Dtos
{
    [Obsolete("BWC for datacontract madness")]
    // This is IPEndPoint really?
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class ServerAddress : IEquatable<ServerAddress>
    {
        [DataMember] IPAddress _ip;
        [DataMember] int _port;
        string _stringFormat;

        public ServerAddress(string address) {
            var addrs = address.Split(':');
            if (addrs.Length < 2)
                throw new Exception("Invalid address format: " + address);

            var port = TryInt(addrs.Last());
            if (port < 1 || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(port.ToString());

            _ip = IPAddress.Parse(string.Join(":", addrs.Take(addrs.Length - 1)));
            _port = port;
            _stringFormat = GetStringFormat();
        }

        public ServerAddress(string ip, int port) {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentNullException("ip");
            if (port < 1 || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(port.ToString());

            _ip = IPAddress.Parse(ip);
            _port = port;

            _stringFormat = GetStringFormat();
        }

        public ServerAddress(IPAddress ip, int port) {
            if (ip == null)
                throw new ArgumentNullException("ip");
            if (port < 1 || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(port.ToString());

            _ip = ip;
            _port = port;

            _stringFormat = GetStringFormat();
        }

        public IPAddress IP
        {
            get { return _ip; }
        }
        public int Port
        {
            get { return _port; }
        }

        public bool Equals(ServerAddress other) {
            if (ReferenceEquals(null, other))
                return false;
            return ReferenceEquals(this, other) || String.Equals(_stringFormat, other._stringFormat);
        }

        string GetStringFormat() {
            return String.Format("{0}:{1}", IP, Port);
        }

        public override int GetHashCode() {
            return (_stringFormat != null ? _stringFormat.GetHashCode() : 0);
        }

        static int TryInt(string val) {
            int result;
            return Int32.TryParse(val, out result) ? result : 0;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ServerAddress) obj);
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            if (_ip == null)
                throw new Exception("IP cant be null");
            if (_port < 1 || _port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(_port.ToString());

            _stringFormat = GetStringFormat();
        }

        public override string ToString() {
            return _stringFormat;
        }
    }


    [DataContract(Name = "UserSettings", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")
    ]
    public class UserSettings
    {
        [DataMember] AccountOptions _accountOptions = new AccountOptions();
        //[DataMember] AppOptions _appOptions = new AppOptions();
        //[DataMember] Version _appVersion;
        [DataMember] GameOptions _gameOptions = new GameOptions();
        //[DataMember] Migrations _migrations = new Migrations();
        //[DataMember] MissionOptions _missionOptions = new MissionOptions();
        [DataMember] ModOptions _modOptions = new ModOptions();
        public ModOptions ModOptions => _modOptions;
        public GameOptions GameOptions => _gameOptions;
        public AccountOptions AccountOptions => _accountOptions;
    }

    [DataContract(Name = "ModOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class ModOptions
    {
        [DataMember] List<CustomCollection> _customModSets = new List<CustomCollection>();
        [DataMember] List<FavoriteMod> _favoriteMods = new List<FavoriteMod>();
        [DataMember] List<FavoriteCollection> _favorites = new List<FavoriteCollection>();
        //[DataMember] List<LocalModsContainer> _localMods = new List<LocalModsContainer>();
        [DataMember] ReactiveList<RecentCollection> _recentModSets = new ReactiveList<RecentCollection>();
        [DataMember] List<SubscribedCollection> _subscribedModSets = new List<SubscribedCollection>();
        public List<CustomCollection> CustomModSets
        {
            get { return _customModSets; }
        }
        public List<FavoriteMod> FavoriteMods
        {
            get { return _favoriteMods; }
        }
        public List<FavoriteCollection> Favorites
        {
            get { return _favorites; }
        }
        public ReactiveList<RecentCollection> RecentModSets
        {
            get { return _recentModSets; }
        }
        public List<SubscribedCollection> SubscribedModSets
        {
            get { return _subscribedModSets; }
        }
    }

    [DataContract(Name = "AccountOptions",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class AccountOptions {}

    [DataContract(Name = "RecentCollection",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class RecentCollection {}

    [DataContract(Name = "FavoriteCollection",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteCollection {}

    [DataContract(Name = "FavoriteMod",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteMod {}

    [DataContract(Name = "SyncBase",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class SyncBase
    {
        [DataMember] Guid _id;
        [DataMember] string _Image;
        [DataMember] string _ImageLarge;
        [DataMember] string _Name;
        public Guid ID
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _Name; }
        }
        public string ImageLarge
        {
            get { return _ImageLarge; }
        }
        public string Image
        {
            get { return _Image; }
        }
    }

    [DataContract(Name = "AdvancedCollection",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class AdvancedCollection : SyncBase
    {
        [DataMember] List<string> _additionalMods;
        [DataMember] protected List<string> _Mods;
        [DataMember] List<string> _optionalMods;
        [DataMember] Guid _realGameUuid;
        [DataMember] List<Uri> _repositories = new List<Uri>();
        [DataMember] List<CollectionServer> _servers = new List<CollectionServer>();
        public Guid GameId
        {
            get { return _realGameUuid; }
        }
        public List<CollectionServer> Servers
        {
            get { return _servers; }
        }
        public List<Uri> Repositories
        {
            get { return _repositories; }
        }
        public List<string> OptionalMods
        {
            get { return _optionalMods; }
        }
        public List<string> AdditionalMods
        {
            get { return _additionalMods; }
        }
        public List<string> Mods
        {
            get { return _Mods; }
        }
    }

    [DataContract(Name = "SubscribedCollection",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class SubscribedCollection : AdvancedCollection
    {
        [DataMember] Guid _collectionID;
        public Guid CollectionID
        {
            get { return _collectionID; }
        }
    }

    [DataContract(Name = "CustomModSet",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class CustomCollection : AdvancedCollection
    {
        [DataMember] string _CustomRepoUrl;
        [DataMember] Guid? _publishedId;
        public string CustomRepoUrl
        {
            get { return _CustomRepoUrl; }
        }
        public Guid? PublishedId
        {
            get { return _publishedId; }
        }
    }

    [DataContract(Name = "CollectionServer",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class CollectionServer
    {
        [DataMember]
        public ServerAddress Address { get; set; } // TODO: This might be problematic to import..
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract(Name = "GameOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GameOptions
    {
        [DataMember] GameSettingsController _gameSettingsController = new GameSettingsController();
        public GameSettingsController GameSettingsController => _gameSettingsController;
    }

    public interface IGetData
    {
        T GetData<T>(Guid gameId, string propertyName);
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public enum ServerQueryMode
    {
        [EnumMember] All = 0,
        [EnumMember] Gamespy = 1,
        [EnumMember] Steam = 2
    }

    [DataContract(Name = "ServerFilter",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Filters")]
    public class ArmaServerFilter {}

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    [KnownType(typeof (GlobalGameSettingsProfile)), KnownType(typeof (GameSettingsProfile)),
     KnownType(typeof (RecentGameSettings)), KnownType(typeof (ServerQueryMode)),
     KnownType(typeof (ProcessPriorityClass))]
    public class GameSettingsController
    {
        [DataMember] Guid? _activeProfileGuid;
        //List<GameSettings> _gameSettings = new List<GameSettings>();
        [DataMember] ReactiveList<GameSettingsProfileBase> _profiles;
        public ReactiveList<GameSettingsProfileBase> Profiles => _profiles;
        public Guid? ActiveProfileGuid => _activeProfileGuid;
        public GameSettingsProfileBase ActiveProfile { get; set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            ActiveProfile = _profiles.FirstOrDefault(x => x.Id == ActiveProfileGuid);
        }

        public T GetValue<T>(Guid game, string propertyName) {
            return
                GetAllProfiles(ActiveProfile).Select(x => x.GetData<T>(game, propertyName))
                    .FirstOrDefault(x => !EqualityComparer<T>.Default.Equals(x, default(T)));
        }

        static IEnumerable<IGetData> GetAllProfiles(GameSettingsProfileBase activeProfile) {
            Contract.Requires<NullReferenceException>(activeProfile != null);

            var profile = activeProfile;
            while (profile != null) {
                yield return profile;
                profile = profile.Parent;
            }
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class RecentGameSettings {}

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    [KnownType(typeof (ArmaServerFilter))]
    public abstract class GameSettingsProfileBase : PropertyChangedBase, IGetData
    {
        [DataMember] readonly Dictionary<Guid, ConcurrentDictionary<string, object>> _gameSettings =
            new Dictionary<Guid, ConcurrentDictionary<string, object>>();
        [DataMember] string _color;
        [DataMember] string _name;
        GameSettingsProfileBase _parent;

        public GameSettingsProfileBase(Guid id, string name, string color) : this(id) {
            _name = name;
            _color = color;
        }

        public GameSettingsProfileBase(Guid id) {
            Id = id;
        }

        public Dictionary<Guid, ConcurrentDictionary<string, object>> GameSettings
        {
            get { return _gameSettings; }
        }
        [DataMember]
        public virtual Guid? ParentId { get; protected set; }
        [DataMember(Name = "Uuid")]
        public virtual Guid Id { get; private set; }
        public virtual bool CanDelete
        {
            get { return true; }
        }
        public virtual string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public virtual string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
        public virtual GameSettingsProfileBase Parent
        {
            get { return _parent; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                _parent = value;
            }
        }

        public T GetData<T>(Guid gameId, string propertyName) {
            var settings = _gameSettings[gameId];
            object propertyValue;
            settings.TryGetValue(propertyName, out propertyValue);
            return propertyValue == null ? default(T) : (T) propertyValue;
        }

        public bool SetData<T>(Guid gameId, string propertyName, T value) {
            var equalityComparer = EqualityComparer<T>.Default;
            if (equalityComparer.Equals(value, GetData<T>(gameId, propertyName)))
                return false;
            if (equalityComparer.Equals(value, default(T))) {
                object currentVal;
                _gameSettings[gameId].TryRemove(propertyName, out currentVal);
            } else
                _gameSettings[gameId][propertyName] = value;

            return true;
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GlobalGameSettingsProfile : GameSettingsProfileBase
    {
        public static readonly Guid GlobalId = new Guid("8b15f343-0ec6-4693-8b30-6508d6c44837");
        public GlobalGameSettingsProfile() : base(GlobalId) {}
        public override Guid Id
        {
            get { return GlobalId; }
        }
        public override string Name
        {
            get { return "Global"; }
            set { }
        }
        public override string Color
        {
            get { return "#146bff"; }
            set { }
        }
        public override GameSettingsProfileBase Parent
        {
            get { return null; }
            set { }
        }
        public override bool CanDelete
        {
            get { return false; }
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GameSettingsProfile : GameSettingsProfileBase
    {
        GameSettingsProfileBase _parent;

        protected GameSettingsProfile(Guid id, string name, string color, GameSettingsProfileBase parent)
            : base(id, name, color) {
            Contract.Requires<ArgumentNullException>(parent != null);
            _parent = parent;

            SetupRefresh();
        }

        public GameSettingsProfile(string name, string color, GameSettingsProfileBase parent)
            : this(Guid.NewGuid(), name, color, parent) {}

        [DataMember(Name = "ParentUuid")]
        public override Guid? ParentId { get; protected set; }
        public override GameSettingsProfileBase Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        void SetupRefresh() {
            this.WhenAnyValue(x => x.Parent)
                .Skip(1)
                .Subscribe(x => Refresh());
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            ParentId = Parent == null ? (Guid?) null : Parent.Id;
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            SetupRefresh();
        }
    }
}