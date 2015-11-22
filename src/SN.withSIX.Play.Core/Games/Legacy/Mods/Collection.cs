// <copyright company="SIX Networks GmbH" file="Collection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DataContract(Name = "ModSet", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    [DoNotObfuscate]
    public class Collection : HostedContent, IComparePK<Collection>, ICopyProperties, ISelectionList<IContent>,
        IEnableLogging,
        IHaveId<Guid>
    {
        public static readonly Guid DefaultGameUuid = GameUuids.Arma2Co;
        static readonly string[] ignoredProperties = {
            "LastJoinedOn", "IsFavorite", "Items",
            "RequiredMods", "Notes", "HasNotes", "AdditionalMods", "DisabledItems", "Dependencies"
        };
        static readonly IPAddress[] ips = new[] {
            "178.33.228.208",
            "46.105.74.59",
            "91.121.100.53",
            "87.98.137.148",
            "172.241.151.34",
            "173.234.43.125"
        }.Select(IPAddress.Parse).ToArray();
        static readonly string[] officialOriginsNames = {"Official Origins Mod", "Official DayZ Origins"};
        [DataMember] List<string> _additionalMods;
        [DataMember] string _ChangelogUrl;
        [DataMember] List<string> _disabledItems;
        [DataMember] string _gameUuid;
        bool? _hasNotes;
        bool? _isFavorite;
        bool _isInstalled;
        [DataMember] protected List<string> _Mods;
        [DataMember] List<string> _optionalMods;
        [DataMember] Guid _realGameUuid = DefaultGameUuid;
        [DataMember] RecentMission _recentMission;
        [DataMember] RecentServer _recentServer;
        [DataMember] List<string> _requiredMods;
        long _size;
        ContentState _state;
        [DataMember] string _SupportUrl;
        //private static readonly ModSetEquals ModSetEquals = new ModSetEquals();
        public int Status = 0;

        public Collection(Guid id, ISupportModding game) : this(id) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game;
            GameId = ((Game) game).Id;
        }

        [Obsolete("Coming from ModSetDto api")]
        public Collection(Guid id) : base(id) {
            _Mods = new List<string>();
            _additionalMods = new List<string>();
            _optionalMods = new List<string>();
            _disabledItems = new List<string>();
            Items = new ReactiveList<IContent>();
            _requiredMods = new List<string>();
            Dependencies = new IMod[0];
            Versions = new Dictionary<string, string>();
            Orders = new Dictionary<string, int>();
        }

        public string DisplayName
        {
            get { return Name; }
        }
        public override string Name
        {
            get { return base.Name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentOutOfRangeException("name", "May not be empty");
                base.Name = value;
            }
        }
        public RecentServer RecentServer
        {
            get { return _recentServer; }
            set { SetProperty(ref _recentServer, value); }
        }
        public RecentMission RecentMission
        {
            get { return _recentMission; }
            set { SetProperty(ref _recentMission, value); }
        }
        public Server ParkedServer { get; set; }
        public MissionBase ParkedMission { get; set; }
        public ContentState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
        public List<string> OptionalMods
        {
            get { return _optionalMods; }
            protected set { SetProperty(ref _optionalMods, value); }
        }
        protected IMod[] Dependencies { get; set; }
        public bool SkipServerMods { get; protected set; }
        public List<string> AdditionalMods
        {
            get { return _additionalMods; }
            set { SetProperty(ref _additionalMods, value); }
        }
        public List<string> DisabledItems
        {
            get { return _disabledItems; }
            set { SetProperty(ref _disabledItems, value); }
        }
        public Guid GameId
        {
            get { return _realGameUuid; }
            private set { _realGameUuid = value; }
        }
        public virtual bool IsCustom
        {
            get { return false; }
        }
        public long Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }
        public IMod[] EnabledMods { get; set; } = new IMod[0];
        public ISupportModding Game { get; private set; }

        protected void Import(Collection src) {
            Name = src.Name;
            Description = src.Description;
            Mods = src.Mods.ToList();
            RequiredMods = src.RequiredMods.ToList();
            AdditionalMods = src.AdditionalMods.ToList();
            OptionalMods = src.OptionalMods.ToList();
            DisabledItems = src.DisabledItems.ToList();
            Dependencies = src.Dependencies.ToArray();
            src.Items.SyncCollection(Items);
        }

        public virtual async Task UpdateInfoFromOnline(CollectionModel collection,
            CollectionVersionModel collectionVersion,
            Account author, IContentManager contentList) {
            var imageUrl = collection.AvatarUrl == null ? null : "http:" + collection.AvatarUrl;
            Image = imageUrl;
            ImageLarge = imageUrl;
            Name = collection.Name;
            Description = collectionVersion.Description;
            Version = collectionVersion.Version.ToString();
            UpdatedAt = collectionVersion.ReleasedOn;
        }

        protected virtual IEnumerable<IMod> ConvertToMods(CollectionVersionModel collectionVersion,
            IContentManager contentList) {
            return contentList.FindOrCreateLocalMods(Game,
                collectionVersion.Dependencies.Select(x => x.Dependency));
        }

        protected virtual async Task SynchronizeMods(IContentManager contentList,
            CollectionVersionModel collectionVersion) {
            foreach (var d in collectionVersion.Dependencies)
                SetVersion(d.Dependency, d.Constraint);
            var theMods = ConvertToMods(collectionVersion, contentList).ToArray();
            RefreshOrders(theMods);
            theMods.Select(x => x.GetSerializationString()).SyncCollection(AdditionalMods);
            RequiredMods =
                theMods.Select(x => x.Name)
                    .Where(
                        x =>
                            collectionVersion.Dependencies.Any(
                                d => d.Dependency.Equals(x, StringComparison.InvariantCultureIgnoreCase) && d.IsRequired))
                    .ToList();
            HandleModsetMods(contentList);
            UpdateState();
        }

        void RefreshOrders(IMod[] theMods) {
            Orders.Clear();
            foreach (var m in theMods.Select((x, i) => new {x, i}))
                SetOrder(m.x, m.i + 1);
        }

        public void SetVersion(string dependency, string constraint) {
            Contract.Requires<ArgumentNullException>(dependency != null);
            Contract.Requires<ArgumentOutOfRangeException>(constraint != string.Empty);
            // TODO: What about deeper check; is this really a version denotation, etc..

            dependency = dependency.ToLower();
            if (constraint == null) {
                if (Versions.ContainsKey(dependency))
                    Versions.Remove(dependency);
                return;
            }
            Versions[dependency] = constraint;
        }

        void RefreshPackageVersions() {
            lock (Items)
                foreach (var m in GetMods().Where(x => x.Controller.Package != null))
                    m.SetPackage();
        }

        public void UpdateState() {
            // TODO: This ignores the active server!
            // TOOD: Integrate ServerMods to ModSets and let them be processed by ModSet.UpdateState()
            // TODO: Consider never working without a modset, so even "No ModSet" is actually a modset? that would solve a lot of null checks and what not

            var game = DomainEvilGlobal.SelectedGame.ActiveGame;
            var isActiveCollection = game.CalculatedSettings.Collection == this;
            if (isActiveCollection)
                RefreshPackageVersions();
            var supportModding = game.Modding();
            UpdateEnabledMods();
            var compatibleEnabledMods = CompatibleEnabledMods(supportModding).ToArray();
            UpdateFromFirstMod();
            var allMods = GetAllEnabledMods(compatibleEnabledMods, supportModding).ToArray();
            supportModding.UpdateModStates(allMods);

            UpdateStatus(allMods);
            if (isActiveCollection)
                game.CalculatedSettings.UpdateMod();
        }

        public void UpdateStatus(Game game) {
            var modding = game.Modding();
            UpdateEnabledMods();
            UpdateStatus(GetAllEnabledMods(CompatibleEnabledMods(modding).ToArray(), modding).ToArray());
        }

        public override Uri GetChangelogUrl() {
            var mod = Items.FirstOrDefault();
            return mod == null ? null : mod.GetChangelogUrl();
        }

        public bool IsOfficialServer(Server server) {
            var customMs = this as CustomCollection;
            if (customMs != null) {
                if (customMs.CustomRepo != null) {
                    var serverName = Path.GetFileNameWithoutExtension(customMs.CustomRepoUrl);
                    return
                        customMs.CustomRepo.Servers.Any(
                            x => x.Key == serverName && server.Address.Equals(x.Value.GetQueryAddress()));
                }
            }

            if (Items.Select(x => x.Name).ContainsIgnoreCase("@dayz")) {
                var serverMods = server.Mods;
                return serverMods != null && serverMods.ContainsIgnoreCase("@hive");
            }

            if (Items.Select(x => x.Name).ContainsIgnoreCase("@dayz_origins")) {
                var name = server.Name;
                return name != null &&
                       officialOriginsNames.Any(x => name.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
            }

            return false;
        }

        public override Uri ProfileUrl() {
            return Tools.Transfer.JoinUri(((Game) Game).GetUri(), "collections", GetShortId(), GetSlug());
        }

        protected virtual ShortGuid GetShortId() {
            return new ShortGuid(Id);
        }

        string GetSlug() {
            return Name.Sluggify();
        }

        public bool AddModAndUpdateState(IEnumerable<IMod> mods, IContentManager modList) {
            lock (Items) {
                var addedMods = mods.Select(AddMod).ToArray();
                if (!addedMods.Any(x => x))
                    return false;
                HandleDependencies(modList);
                UpdateState();
                return true;
            }
        }

        public bool RemoveModAndUpdateState(IEnumerable<IMod> mods, IContentManager modList) {
            lock (Items) {
                var removedMods = mods.Select(RemoveMod).ToArray();
                if (!removedMods.Any(x => x))
                    return false;
                HandleDependencies(modList);
                UpdateState();

                return true;
            }
        }

        public bool AddModAndUpdateStateIfPersistent(IEnumerable<IMod> mods, IContentManager modList) {
            if (IsNotPersistent())
                return false;
            lock (Items) {
                var addedMods = mods.Select(AddMod).ToArray();
                if (!addedMods.Any(x => x))
                    return false;
                HandleDependencies(modList);
                UpdateState();
                return true;
            }
        }

        public bool RemoveModAndUpdateStateIfPersistent(IReadOnlyCollection<IMod> mods, IContentManager modList) {
            if (IsNotPersistent())
                return false;
            lock (Items) {
                var removedMods = mods.Select(RemoveMod).ToArray();
                if (!removedMods.Any(x => x))
                    return false;
                HandleDependencies(modList);
                UpdateState();

                return true;
            }
        }

        public bool AddModAndUpdateStateIfPersistent(IMod mod, IContentManager modList) {
            if (IsNotPersistent())
                return false;
            if (!AddMod(mod))
                return false;
            HandleDependencies(modList);
            UpdateState();
            return true;
        }

        public bool RemoveModAndUpdateStateIfPersistent(IMod mod, IContentManager modList) {
            if (IsNotPersistent())
                return false;
            if (!RemoveMod(mod))
                return false;
            HandleDependencies(modList);
            UpdateState();
            return true;
        }

        public bool AddModAndUpdateState(IMod mod, IContentManager modList) {
            if (!AddMod(mod))
                return false;
            HandleDependencies(modList);
            UpdateState();
            return true;
        }

        public bool RemoveModAndUpdateState(IMod mod, IContentManager modList) {
            if (!RemoveMod(mod))
                return false;
            HandleDependencies(modList);
            UpdateState();
            return true;
        }

        public virtual void UpdateFromMod(IMod mod) {
            var m = mod as Mod;
            Version = mod.ModVersion;
            Categories = m == null ? null : m.Categories;
            if (!string.IsNullOrWhiteSpace(mod.Image))
                Image = mod.Image;
            if (!string.IsNullOrWhiteSpace(mod.ImageLarge))
                ImageLarge = mod.ImageLarge;
            UpdatedAt = mod.UpdatedVersion;
            if (String.IsNullOrWhiteSpace(ChangelogUrl))
                ChangelogUrl = mod.GetUrl();
            if (String.IsNullOrWhiteSpace(HomepageUrl))
                HomepageUrl = mod.HomepageUrl;
        }

        public bool IsFeaturedServer(Server server) {
            switch (Id.ToString()) {
            case "901a1758-8e01-4671-a77b-104b6ad801e8": {
                var ip = server.Address.IP;
                return ip != null && ips.Any(x => x.Equals(ip));
            }
            }

            return false;
        }

        public bool GameMatch(Game game) {
            Contract.Requires<ArgumentNullException>(game != null);
            return game.Id.Equals(GameId);
        }

        public bool GameMatch(ISupportModding game) {
            return GameMatch((Game) game);
        }

        public IMod GetFirstMod() {
            return EnabledMods.FirstOrDefault(x => !(x is LocalMod)) ?? EnabledMods.FirstOrDefault();
        }

        public IEnumerable<IMod> ModItems() {
            return GetMods().Select(x => x.Model);
        }

        protected IEnumerable<ToggleableModProxy> GetMods() {
            return Items.OfType<ToggleableModProxy>().OrderBy(x => x.Order.GetValueOrDefault(Int32.MaxValue));
        }

        void UpdateEnabledMods() {
            lock (Items)
                EnabledMods = GetMods().Where(x => x.IsEnabled).Select(x => x.Model).ToArray();
        }

        public virtual CustomCollection Clone() {
            var cm = new CustomCollection(Guid.NewGuid(), Game);
            cm.Import(this);
            return cm;
        }

        public void EnableMod(string name) {
            DisabledItems.RemoveAll(x => x.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            UpdateState();
        }

        public void DisableMod(string name) {
            DisabledItems.Add(name);
            UpdateState();
        }

        public void Process(IContentManager modList) {
            Contract.Requires<ArgumentNullException>(modList != null);
            HandleModsetMods(modList);
            if (String.IsNullOrWhiteSpace(SupportUrl))
                SupportUrl = HomepageUrl;
        }

        public void ChangeGame(Game game) {
            Contract.Requires<ArgumentNullException>(game != null);
            Game = game.Modding();
            GameId = game.Id;
        }

        public void Clear(IContentManager modList) {
            lock (Items)
                RemoveModAndUpdateState(ModItems().ToArray(), modList);
        }

        public void ClearCustomizations(IContentManager modList) {
            DisabledItems.Clear();
            AdditionalMods.Clear();
            HandleDependencies(modList);
            UpdateState();
        }

        IEnumerable<IMod> CompatibleEnabledMods(ISupportModding game) {
            return CalculatedGameSettings.ContentManager.CompatibleMods(EnabledMods, game);
        }

        protected virtual void HandleModsetMods(IContentManager modList) {
            HandleModsetModsInternal(modList);
        }

        protected virtual IEnumerable<string> GetAllowedMods() {
            return GetAllContainedMods();
        }

        protected virtual IEnumerable<string> GetCleanedModList() {
            return GetAllowedMods();
        }

        IEnumerable<string> GetAllContainedMods() {
            return Mods.Concat(AdditionalMods);
        }

        protected virtual void HandleDependencies(IContentManager modList) {
            string[] mods;
            lock (Items)
                mods = ModItems().Select(x => x.GetSerializationString()).ToArray();

            var dependencies = modList.GetDependencies(mods);
            var dependendMods = modList.FindOrCreateLocalMods(Game, dependencies)
                .ToArray();
            Dependencies = dependendMods;
        }

        protected void HandleModsetModsInternal(IContentManager modList, IReadOnlyCollection<IMod> inputMods = null) {
            var allowedMods = GetCleanedModList().ToArray();
            var foundMods = modList.FindOrCreateLocalMods(Game, allowedMods, inputMods)
                .ToArray();

            ReplaceMods(foundMods);
            HandleDependencies(modList);

            lock (Items)
                foreach (var item in GetMods())
                    item.RefreshInfo();
        }

        bool IsNotPersistent() {
            return Id == Guid.Empty;
        }

        IEnumerable<IMod> GetAllEnabledMods(IMod[] enabledMods, ISupportModding gameSet) {
            return enabledMods.Concat(EnabledDependencies(enabledMods, gameSet)).Distinct();
        }

        void ReplaceMods(IEnumerable<IMod> foundMods) {
            ToggleableModProxy[] currentItems;
            lock (Items)
                currentItems = GetMods().ToArray();
            foundMods.Select(GetToggableModProxy).SyncCollectionLocked(Items);
            currentItems.ForEach(x => x.Dispose());
        }

        IEnumerable<IMod> EnabledDependencies(IEnumerable<IMod> enabledMods, ISupportModding gameSet) {
            return
                Dependencies.Where(x => enabledMods.Any(y => y.Dependencies.Any(x.Match)) && x.CompatibleWith(gameSet));
        }

        void UpdateStatus(IMod[] allMods) {
            State = GetState(allMods);
            Size = allMods.Sum(x => x.Size);
            IsInstalled = State != ContentState.NotInstalled;
        }

        protected virtual void UpdateFromFirstMod() {
            var firstMod = GetFirstMod();
            if (firstMod != null)
                UpdateFromMod(firstMod);
        }

        protected virtual bool AddMod(IMod mod) {
            var key = mod.GetSerializationString();
            lock (Items) {
                if (Mods.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    return false;
                if (AdditionalMods.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    return false;
                AdditionalMods.Add(key);
                Items.Add(GetToggableModProxy(mod));
                return true;
            }
        }

        protected virtual ToggleableModProxy GetToggableModProxy(IMod mod) {
            return new ToggleableModProxy(mod, this);
        }

        protected virtual bool RemoveMod(IMod mod) {
            var key = mod.GetSerializationString();
            lock (Items) {
                if (!AdditionalMods.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    return false;
                AdditionalMods.Remove(key);
                if (Mods.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    return false;
                Items.RemoveAll(x => x.ComparePK(mod));
                DisabledItems.RemoveAll(x => x.Equals(mod.Name, StringComparison.InvariantCultureIgnoreCase));
                UpdateState();
                return true;
            }
        }

        static ContentState GetState(IMod[] enabledMods) {
            if (enabledMods.Any(x => x.State == ContentState.UpdateAvailable))
                return ContentState.UpdateAvailable;
            if (enabledMods.Any(x => x.State == ContentState.NotInstalled))
                return ContentState.NotInstalled;
            return enabledMods.Any(x => x.State == ContentState.Unverified)
                ? ContentState.Unverified
                : ContentState.Uptodate;
        }

        // TODO: Still in place for CustomModSets - probably better to make a DTO for it anyway...
        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (!string.IsNullOrWhiteSpace(_gameUuid)) {
                _realGameUuid = FindGameUuidMatch(_gameUuid).GetValueOrDefault();
                _gameUuid = null;
            }

            if (EnabledMods == null)
                EnabledMods = new IMod[0];

            if (_realGameUuid == default(Guid))
                _realGameUuid = DefaultGameUuid;
            if (_Mods == null)
                _Mods = new List<string>();
            if (_additionalMods == null)
                _additionalMods = new List<string>();
            if (_optionalMods == null)
                _optionalMods = new List<string>();
            if (_disabledItems == null)
                _disabledItems = new List<string>();

            if (RequiredMods == null)
                RequiredMods = new List<string>();

            if (Dependencies == null)
                Dependencies = new IMod[0];

            if (Items == null)
                Items = new ReactiveList<IContent>();

            if (Versions == null)
                Versions = new Dictionary<string, string>();
            if (Orders == null)
                Orders = new Dictionary<string, int>();

            // WORKAROUND F'ING DATALOSS POSSIBILITIES
            if (_recentMission != null && _recentMission.Key == null)
                _recentMission = null;
            if (_recentServer != null && _recentServer.Address == null)
                _recentServer = null;
        }

        static Guid? FindGameUuidMatch(string gameName) {
            switch (gameName.ToLower()) {
            case "arma 3":
                return GameUuids.Arma3;
            case "arma3":
                return GameUuids.Arma3;
            case "arma 2":
                return GameUuids.Arma2;
            case "arma2":
                return GameUuids.Arma2;
            case "arma 2 oa":
                return GameUuids.Arma2Oa;
            case "arma2oa":
                return GameUuids.Arma2Oa;
            case "arma 2 co":
                return GameUuids.Arma2Co;
            case "arma2co":
                return GameUuids.Arma2Co;
            default: {
                Guid id;
                return Guid.TryParse(gameName, out id) ? (Guid?) id : null;
            }
            }
        }

        public Dependency GetDesiredModVersion(ToggleableModProxy mod) {
            var name = mod.GetSerializationString().ToLower();
            return Versions.ContainsKey(name)
                ? new Dependency(name, Versions[name])
                : new GlobalDependency(name);
        }

        public int? GetOrder(ToggleableModProxy mod) {
            var name = mod.GetSerializationString().ToLower();
            return Orders.ContainsKey(name)
                ? (int?) Orders[name]
                : null;
        }

        public void SetOrder(IMod mod, int? order) {
            var name = mod.GetSerializationString().ToLower();
            if (order == null) {
                if (Orders.ContainsKey(name))
                    Orders.Remove(name);
            } else
                Orders[name] = order.GetValueOrDefault();
        }

        #region ModSet Members

        public string ChangelogUrl
        {
            get { return _ChangelogUrl; }
            set { SetProperty(ref _ChangelogUrl, value); }
        }

        public string SupportUrl
        {
            get { return _SupportUrl; }
            set { SetProperty(ref _SupportUrl, value); }
        }

        // TODO: Clean this stuff out...? AdditionalMods, vs Mods, vs OptionalMods vs RequiredMods etc
        public List<string> Mods
        {
            get { return _Mods; }
            set { SetProperty(ref _Mods, value); }
        }

        public DateTime? LastJoinedOn
        {
            get
            {
                var recent = DomainEvilGlobal.Settings.ModOptions.RecentCollections
                    .OrderByDescending(x => x.On)
                    .FirstOrDefault(x => x.Matches(this));
                if (recent == null)
                    return null;

                return recent.On;
            }
        }

        public override bool IsFavorite
        {
            get
            {
                return _isFavorite == null
                    ? (bool) (_isFavorite = DomainEvilGlobal.Settings.ModOptions.IsFavorite(this))
                    : (bool) _isFavorite;
            }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.ModOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.ModOptions.RemoveFavorite(this);
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        public override string Notes
        {
            get { return DomainEvilGlobal.NoteStorage.GetNotes(this); }
            set
            {
                DomainEvilGlobal.NoteStorage.SetNotes(this, value);
                _hasNotes = !string.IsNullOrEmpty(value);
                new[] {"Notes", "HasNotes"}.ForEach(OnPropertyChanged);
            }
        }

        public override bool HasNotes
        {
            get
            {
                TryLoadHasNotes();
                if (_hasNotes != null)
                    return (bool) _hasNotes;
                return false;
            }
        }

        public List<string> RequiredMods
        {
            get { return _requiredMods; }
            set { SetProperty(ref _requiredMods, value); }
        }

        public bool ComparePK(Collection other) {
            return ComparePK((SyncBase) other);
        }

        public string[] IgnoredProperties
        {
            get { return ignoredProperties; }
        }
        public IContent SelectedItem { get; set; }

        public ReactiveList<IContent> Items { get; set; }

        [DataMember]
        public Dictionary<string, string> Versions { get; set; }

        [DataMember]
        public Dictionary<string, int> Orders { get; set; }
        public virtual bool CanChangeRequiredState
        {
            get { return true; }
        }

        public void RefreshLastJoinedOn() {
            OnPropertyChanged("LastJoinedOn");
        }

        void TryLoadHasNotes() {
            try {
                _hasNotes = DomainEvilGlobal.NoteStorage.HasNotes(this);
            } catch (Exception e) {
                this.Logger().FormattedDebugException(e);
            }
        }

        #endregion
    }

    public class BuiltInCollection : Collection
    {
        public BuiltInCollection(ISupportModding game) : base(Guid.Empty, game) {}
        protected override void UpdateFromFirstMod() {}
    }
}