// <copyright company="SIX Networks GmbH" file="CustomCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Connect.Infrastructure.Components;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Sync.Core;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DataContract(Name = "CustomModSet",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    [DoNotObfuscate]
    public class CustomCollection : AdvancedCollection, IComparePK<CustomCollection>, IHaveCustomRepo
    {
        static readonly Version defaultVersion = new Version("0.0.1");
        [DataMember] bool _allModsLocked;
        [DataMember] bool _allModsRequired;
        SixRepo _customRepo;
        SixRepoApp[] _customRepoApps;
        [DataMember] string _CustomRepoUrl;
        [DataMember] string _CustomRepoUuid;
        string _errorMessage;
        [DataMember] Guid? _forkedCollectionId;
        [DataMember] bool? _isOpen = true;
        [DataMember] DateTime? _lastSyncedAt;
        [DataMember] Guid? _publishedAccountId;
        [DataMember] Guid? _publishedId;
        [DataMember] Version _publishedVersion;
        [DataMember] CollectionScope? _publishingScope;
        [DataMember] bool _RememberWarnOnRepoAvailabilty;
        bool _serverFoundAndValid = true;
        [DataMember] int? _subscribers;
        bool _userInitiatedSync;
        public CustomCollection(Guid id, ISupportModding game) : base(id, game) {}
        public DateTime? LastSyncedAt
        {
            get { return _lastSyncedAt; }
            set { SetProperty(ref _lastSyncedAt, value); }
        }
        public Guid? PublishedAccountId
        {
            get { return _publishedAccountId; }
        }
        public bool RememberWarnOnRepoAvailabilty
        {
            get { return _RememberWarnOnRepoAvailabilty; }
            set { SetProperty(ref _RememberWarnOnRepoAvailabilty, value); }
        }
        public bool AllModsLocked
        {
            get { return _allModsLocked; }
            set { SetProperty(ref _allModsLocked, value); }
        }
        public bool AllModsRequired
        {
            get { return _allModsRequired; }
            set { SetProperty(ref _allModsRequired, value); }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }
        public bool UserInitiatedSync
        {
            get { return _userInitiatedSync; }
            set { SetProperty(ref _userInitiatedSync, value); }
        }
        public bool ServerFoundAndValid
        {
            get { return _serverFoundAndValid; }
            set { SetProperty(ref _serverFoundAndValid, value); }
        }
        public bool IsOpen
        {
            get { return _isOpen.GetValueOrDefault(true); }
            set { SetProperty(ref _isOpen, value); }
        }
        public bool ForceModUpdate { get; set; }
        public override bool IsCustom
        {
            get { return true; }
        }
        public string ServerKey { get; set; }
        public bool IsHidden { get; set; }
        public CollectionScope? PublishingScope
        {
            get { return _publishingScope; }
            set { SetProperty(ref _publishingScope, value); }
        }
        public Version PublishedVersion
        {
            get { return _publishedVersion; }
            set { SetProperty(ref _publishedVersion, value); }
        }
        public Guid? PublishedId
        {
            get { return _publishedId; }
            private set
            {
                if (!SetProperty(ref _publishedId, value))
                    return;
                OnPropertyChanged("IsPublished");
            }
        }
        public int? Subscribers
        {
            get { return _subscribers; }
            set { SetProperty(ref _subscribers, value); }
        }
        public Guid? ForkedCollectionId
        {
            get { return _forkedCollectionId; }
            private set { _forkedCollectionId = value; }
        }

        public override bool ComparePK(object obj) {
            var emp = obj as CustomCollection;
            return emp != null && ComparePK(emp);
        }

        public bool ComparePK(CustomCollection other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return (other.CustomRepoUuid != default(string) && other.CustomRepoUuid.Equals(CustomRepoUuid))
                   || (other.CustomRepoUrl != default(string) && other.CustomRepoUrl.Equals(CustomRepoUrl))
                   || (other.Id != Guid.Empty && other.Id.Equals(Id));
        }

        public SixRepo CustomRepo
        {
            get { return _customRepo; }
            set { SetProperty(ref _customRepo, value); }
        }
        public string CustomRepoUrl
        {
            get { return _CustomRepoUrl; }
            set { SetProperty(ref _CustomRepoUrl, value); }
        }
        public string CustomRepoUuid
        {
            get { return _CustomRepoUuid; }
            set { SetProperty(ref _CustomRepoUuid, value); }
        }
        public SixRepoApp[] CustomRepoApps
        {
            get { return _customRepoApps; }
            set { SetProperty(ref _customRepoApps, value); }
        }

        public void Lock() {
            AllModsLocked = true;
            lock (Items)
                foreach (var mod in GetMods()
                    .Where(
                        x =>
                            !(x.ToMod() is CustomRepoMod) && !(x.ToMod() is LocalMod) &&
                            x.DesiredVersion is GlobalDependency))
                    mod.Lock();
        }

        public void Unlock() {
            AllModsLocked = false;
            lock (Items)
                foreach (var mod in GetMods()
                    .Where(
                        x =>
                            !(x.ToMod() is CustomRepoMod) && !(x.ToMod() is LocalMod) &&
                            !(x.DesiredVersion is GlobalDependency)))
                    mod.Unlock();
        }

        public void UpdateAllLocked() {
            lock (Items)
                foreach (var mod in GetMods()
                    .Where(
                        x =>
                            !(x.ToMod() is CustomRepoMod) && !(x.ToMod() is LocalMod) &&
                            !(x.DesiredVersion is SpecializedDependency)))
                    mod.Lock();
        }

        public void Require() {
            AllModsRequired = true;
            lock (Items)
                foreach (var mod in GetMods().Where(x => !(x.ToMod() is CustomRepoMod)))
                    MakeRequired(mod);
        }

        void MakeRequired(ToggleableModProxy mod) {
            var serializationString = mod.GetSerializationString();
            RequiredMods.AddWhenMissing(serializationString);
            OptionalMods.Remove(serializationString);
            mod.IsRequired = true;
        }

        public bool AllowChanges() {
            return ((CustomRepoUrl == null || IsOpen) && !Repositories.Any()) || PublishedId != null;
        }

        public void Unrequire() {
            AllModsRequired = false;
            lock (Items)
                foreach (var mod in GetMods())
                    MakeUnrequired(mod);
        }

        void MakeUnrequired(ToggleableModProxy mod) {
            var serializationString = mod.GetSerializationString();
            // tsk
            RequiredMods.Remove(serializationString);
            OptionalMods.AddWhenMissing(serializationString);
            mod.IsRequired = false;
        }

        public bool RememberWarn() {
            return RememberWarnOnRepoAvailabilty || (CustomRepo != null && CustomRepo.RememberWarnOnRepoAvailabilty);
        }

        public void SetRemember() {
            RememberWarnOnRepoAvailabilty = true;
            if (CustomRepo != null)
                CustomRepo.RememberWarnOnRepoAvailabilty = true;
        }

        internal void Import(SubscribedCollection src) {
            base.Import(src);
            ForkedCollectionId = src.CollectionID;
        }

        void Import(CustomCollection src) {
            base.Import(src);
            ForkedCollectionId = src.PublishedId;
            CustomRepoUrl = src.CustomRepoUrl;
            CustomRepoUuid = src.CustomRepoUuid;
            CustomRepo = src.CustomRepo;
            Repositories = src.Repositories.ToList();
            Servers = src.Servers.ToList();
            ServerKey = src.ServerKey;
        }

        public override CustomCollection Clone() {
            var cm = new CustomCollection(Guid.NewGuid(), Game);
            cm.Import(this);

            return cm;
        }

        public override Uri ProfileUrl() {
            if (PublishedId.HasValue)
                return GetOnlineUrl();
            var repo = CustomRepo;
            return repo != null ? repo.GetInfoUri(ServerKey) : GetOnlineUrl();
        }

        Uri GetOnlineUrl() {
            return Tools.Transfer.JoinUri(((Game) Game).GetUri(), "collections", GetShortId(), GetSlug());
        }

        public override void UpdateFromMod(IMod mod) {}

        protected override ShortGuid GetShortId() {
            return new ShortGuid(PublishedId.Value);
        }

        public bool HasCustomRepo() {
            return CustomRepoUrl != null;
        }

        public void UpdateCustomRepoInfo(IContentManager modList, string url, SixRepo repo) {
            Contract.Requires<ArgumentNullException>(modList != null);
            Contract.Requires<ArgumentNullException>(url != null);
            Contract.Requires<ArgumentNullException>(repo != null);

            UpdateSharedRepoInfo(url, repo);

            GetNameFromRepo(repo);

            HandleModsetMods(modList);
        }

        public void UpdateCustomRepoServerInfo(IContentManager modList, SixRepoServer repoServer, string url,
            SixRepo repo) {
            Contract.Requires<ArgumentNullException>(modList != null);
            Contract.Requires<ArgumentNullException>(repoServer != null);
            Contract.Requires<ArgumentNullException>(url != null);
            Contract.Requires<ArgumentNullException>(repo != null);

            UpdateSharedRepoInfo(url, repo);

            var rMods = repoServer.RequiredMods;
            var optionalMods =
                repoServer.AllowedMods.Where(x => !SixRepoServer.SYS.Contains(x))
                    .DistinctBy(x => x.ToLower())
                    .ToList();
            var requiredMods =
                rMods.Where(x => !SixRepoServer.SYS.Contains(x)).DistinctBy(x => x.ToLower()).ToArray();
            Name = GetNameFromServerOrRepo(repoServer, repo);
            Mods = requiredMods.ToList();
            SkipServerMods = rMods.None(SixRepoServer.SYS.Contains);
            RequiredMods =
                requiredMods.Where(x => !optionalMods.Contains(x, StringComparer.CurrentCultureIgnoreCase)).ToList();
            OptionalMods = optionalMods;
            DisabledItems.AddWhenMissing(optionalMods.Where(x =>
                !requiredMods.Contains(x, StringComparer.CurrentCultureIgnoreCase)
                && !AdditionalMods.Contains(x, StringComparer.CurrentCultureIgnoreCase)),
                StringComparison.CurrentCultureIgnoreCase);

            AdditionalMods.AddWhenMissing(optionalMods, StringComparison.CurrentCultureIgnoreCase);

            Image = String.IsNullOrWhiteSpace(repoServer.Image) ? repo.Config.Image : repoServer.Image;
            ImageLarge =
                String.IsNullOrWhiteSpace(repoServer.ImageLarge)
                    ? repo.Config.ImageLarge
                    : repoServer.ImageLarge;

            CustomRepoUuid = repoServer.Uuid;
            CustomRepoApps = repoServer.Apps.Where(x => repo.Config.Apps.ContainsKey(x))
                .Select(x => repo.Config.Apps[x]).ToArray();
            IsOpen = repoServer.IsOpen;
            ForceModUpdate = repoServer.ForceModUpdate;

            HandleModsetMods(modList);
            UpdateState();
        }

        void UpdateSharedRepoInfo(string url, SixRepo repo) {
            CustomRepo = repo;
            CustomRepoUrl = url;
            CustomRepoMods = repo.Mods;
            HomepageUrl = repo.Config.Homepage;
        }

        static string GetNameFromServerOrRepo(SixRepoServer repoServer, SixRepo repo) {
            return String.IsNullOrWhiteSpace(repoServer.Name) ? GetNameFromRepo(repo) : repoServer.Name;
        }

        static string GetNameFromRepo(SixRepo repo) {
            return String.IsNullOrWhiteSpace(repo.Config.Name)
                ? "Custom Repo Collection (NAME NOT DEFINED)"
                : repo.Config.Name;
        }

        public string RefreshRepoInfo(SixRepo repo, string serverName) {
            if (!repo.Servers.ContainsKey(serverName))
                return HandleServerMissingFromRepo(serverName);

            Author = repo.Name;

            var repoServer = repo.Servers[serverName];
            var errors = repo.ValidateServerConfig(repoServer);
            return errors.Length > 0 ? HandleServerValidationErrors(serverName, errors) : null;
        }

        protected override IEnumerable<string> GetAllowedMods() {
            var allowedMods = base.GetAllowedMods();
            if (AllowsAnyMod())
                return allowedMods;
            var requiredAndAllowed = GetRequiredAndOptionalMods();
            return allowedMods.Where(requiredAndAllowed.ContainsIgnoreCase);
        }

        IEnumerable<string> GetRequiredAndOptionalMods() {
            return RequiredMods.Concat(OptionalMods).ToArray();
        }

        protected override IEnumerable<string> GetCleanedModList() {
            var mods = GetAllowedMods();
            if (IsOpen || !OptionalAllAllowed())
                return mods;

            return KeepModsWhichAreRequiredOrOptionalOrEnabled(mods);
        }

        IEnumerable<string> KeepModsWhichAreRequiredOrOptionalOrEnabled(IEnumerable<string> mods) {
            return mods.Where(x => GetRequiredAndOptionalMods().ContainsIgnoreCase(x)
                                   || !DisabledItems.ContainsIgnoreCase(x));
        }

        bool AllowsAnyMod() {
            return IsOpen || OptionalAllAllowed();
        }

        bool OptionalAllAllowed() {
            return OptionalMods.Contains(":all");
        }

        protected override bool AddMod(IMod mod) {
            if (AllowsAnyMod())
                return base.AddMod(mod);

            return GetRequiredAndOptionalMods().ContainsIgnoreCase(mod.Name) && base.AddMod(mod);
        }

        protected override bool RemoveMod(IMod mod) {
            if (IsOpen)
                return base.RemoveMod(mod);

            return !GetRequiredAndOptionalMods().ContainsIgnoreCase(mod.Name) && base.RemoveMod(mod);
        }

        protected override ToggleableModProxy GetToggableModProxy(IMod mod) {
            var toggleableModProxy = base.GetToggableModProxy(mod);
            if (AllModsLocked)
                toggleableModProxy.Lock();
            if (AllModsRequired)
                MakeRequired(toggleableModProxy);
            return toggleableModProxy;
        }

        string HandleServerValidationErrors(string serverName, IEnumerable<ValidationErrors> errors) {
            var errorMessage = String.Format("CustomRepo failed to validate {0}, {1}", serverName,
                String.Join("\n", errors.Select(x => x.Message)));
            this.Logger().FormattedWarnException(new Exception(errorMessage));
            ServerFoundAndValid = false;
            ErrorMessage = errorMessage;
            if (!UserInitiatedSync)
                return "";

            UserInitiatedSync = false;
            return errorMessage;
        }

        string HandleServerMissingFromRepo(string serverName) {
            var errorMessage = "The repository appears to contain invalid data. Server not found: " +
                               serverName;
            this.Logger().FormattedWarnException(new ConfigException(errorMessage));
            ServerFoundAndValid = false;
            ErrorMessage = errorMessage;
            if (!UserInitiatedSync)
                return "";

            UserInitiatedSync = false;
            return errorMessage;
        }

        protected override void UpdateFromFirstMod() {
            if (CustomRepo == null)
                base.UpdateFromFirstMod();
        }

        public override async Task UpdateInfoFromOnline(CollectionModel collection,
            CollectionVersionModel collectionVersion,
            Account author, IContentManager contentList) {
            await base.UpdateInfoFromOnline(collection, collectionVersion, author, contentList).ConfigureAwait(false);

            Subscribers = collection.Scope == CollectionScope.Private ? (int?) null : collection.Subscribers;
            if (PublishedVersion == null || PublishedVersion < collectionVersion.Version) {
                collectionVersion.Repositories.SyncCollection(Repositories);
                UpdateServersInfo(collectionVersion);
                await SynchronizeMods(contentList, collectionVersion).ConfigureAwait(false);
            }

            // Has to come afterwards because otherwise we don't sync mods on changes...
            SetPublishedInformation(collection, collectionVersion);
        }

        public void SetPublishedInformation(CollectionModel collection, CollectionVersionModel collectionVersion) {
            if (PublishedId.HasValue && PublishedId.Value != collection.Id) {
                throw new Exception(
                    "This modSet already has publishing information set and it's id does not match the provided collections id.");
            }
            PublishedId = collection.Id;
            PublishingScope = collection.Scope;
            UpdatePublishInfo(collectionVersion.Version);
            _publishedAccountId = collection.AuthorId;
        }

        void UpdatePublishInfo(CollectionScope scope, Version version, CollectionPublishInfo publishInfo) {
            PublishedId = publishInfo.Id;
            _publishedAccountId = publishInfo.AccountId;
            PublishingScope = scope;
            UpdatePublishInfo(version);
        }

        public async Task DeleteOnline(IConnectApiHandler api, IContentManager modList) {
            await api.DeleteCollection(PublishedId.Value).ConfigureAwait(false);
            _publishedAccountId = null;
            PublishedVersion = null;
            Version = null;
            _publishingScope = null;
            PublishedId = null;
            HandleModsetMods(modList);
        }

        public override async Task HandleCustomRepositories(IContentManager manager, bool report) {
            if (!HasCustomRepo()) {
                await base.HandleCustomRepositories(manager, report).ConfigureAwait(false);
                return;
            }
            await manager.ProcessLegacyCustomCollection(this, report).ConfigureAwait(false);
            UpdateState();
        }

        public async Task Publish(IConnectApiHandler api, IContentManager modList,
            CollectionScope scope = CollectionScope.Unlisted,
            Guid? forkedCollectionId = null) {
            if (string.IsNullOrWhiteSpace(Name))
                throw new CollectionNameMissingException();
            //if (scope != CollectionScope.Private && string.IsNullOrWhiteSpace(Description))
            // throw new CollectionDescriptionMissingException();
            if (!Items.Any())
                throw new CollectionEmptyException();

            var version = defaultVersion;
            List<CollectionVersionDependencyModel> dependencies;
            lock (Items)
                dependencies = GetMods()
                    .Select(Convert)
                    .ToList();

            var hasCustomRepo = HasCustomRepo();
            if (hasCustomRepo)
                UpdateRepositoriesFromCustomRepo();

            var servers = GetServersForPublishing();

            var publishInfo =
                await api.PublishCollection(new CreateCollectionModel {
                    GameId = GameId,
                    Name = Name,
                    Scope = scope,
                    ForkedCollectionId = forkedCollectionId,
                    InitialVersion = new CreateCollectionVersionModel {
                        Description = Description,
                        Version = version,
                        Dependencies = dependencies,
                        Repositories = Repositories.ToList(),
                        Servers = servers
                    }
                }).ConfigureAwait(false);

            if (hasCustomRepo)
                CleanupRepo();

            UpdatePublishInfo(scope, version, publishInfo);
            HandleModsetMods(modList);
            if (hasCustomRepo)
                await UploadImageIfAvailable(api).ConfigureAwait(false);
        }

        void CleanupRepo() {
            CustomRepoUrl = new Uri(CustomRepoUrl).ProcessRepoUrl().ToString();
            ServerKey = null;
            IsOpen = true;
        }

        async Task UploadImageIfAvailable(IConnectCollectionsApi api) {
            var image = ImageLarge ?? Image;
            if (image != null && Uri.IsWellFormedUriString(image, UriKind.Absolute))
                await UploadImage(image, api).ConfigureAwait(false);
        }

        async Task UploadImage(string image, IConnectCollectionsApi api) {
            try {
                // todo: move into the api or ?
                var tmpFile = Path.GetTempPath().ToAbsoluteDirectoryPath().GetChildFileWithName(Path.GetFileName(image));
                await
                    SyncEvilGlobal.FileDownloader.DownloadAsync(new FileDownloadSpec(new Uri(image), tmpFile))
                        .ConfigureAwait(false);
                await api.UploadCollectionAvatar(tmpFile, PublishedId.Value).ConfigureAwait(false);
            } catch (Exception ex) {
                throw new CollectionImageUploadException("A problem occurred trying to publish the image", ex);
            }
        }

        List<CollectionVersionServerModel> GetServersForPublishing() {
            var servers = new List<CollectionVersionServerModel>();
            if (CustomRepo != null && ServerKey != null) {
                var server = CustomRepo.Servers[ServerKey];
                var queryAddress = server.Address.FixArmaQueryAddress();

                servers.Add(new CollectionVersionServerModel {
                    Address = queryAddress.ToString(),
                    Password = server.Password
                });
                Servers =
                    servers.Select(
                        x => new CollectionServer {Address = new ServerAddress(x.Address), Password = x.Password})
                        .ToList();
            } else {
                servers =
                    Servers.Select(
                        x => new CollectionVersionServerModel {Address = x.Address.ToString(), Password = x.Password})
                        .ToList();
            }
            return servers;
        }

        public async Task PublishNewVersion(IConnectApiHandler api) {
            if (PublishedId == null)
                throw new Exception("Not a published collection");
            if (string.IsNullOrWhiteSpace(Name))
                throw new CollectionNameMissingException();
            //if (scope != CollectionScope.Private && string.IsNullOrWhiteSpace(Description))
            // throw new CollectionDescriptionMissingException();
            if (!Items.Any())
                throw new CollectionEmptyException();

            await api.ChangeCollectionName(PublishedId.Value, Name).ConfigureAwait(false);

            var version = PublishedVersion.AutoIncrement();

            var hasCustomRepo = HasCustomRepo();
            if (hasCustomRepo)
                UpdateRepositoriesFromCustomRepo();

            var servers = GetServersForPublishing();

            await api.PublishNewCollectionVersion(new AddCollectionVersionModel {
                Description = Description,
                CollectionId = PublishedId.Value,
                Version = version,
                Dependencies = Items.OfType<ToggleableModProxy>()
                    .Select(Convert)
                    .ToList(),
                Repositories = Repositories.ToList(),
                Servers = servers
            }).ConfigureAwait(false);

            if (hasCustomRepo)
                CleanupRepo();

            UpdatePublishInfo(version);

            if (hasCustomRepo)
                await UploadImageIfAvailable(api).ConfigureAwait(false);
        }

        static CollectionVersionDependencyModel Convert(ToggleableModProxy x) {
            return new CollectionVersionDependencyModel {
                Dependency = x.Name,
                Constraint = x.GetDesiredOrGlobal(),
                IsRequired = x.IsRequired
            };
        }

        void UpdatePublishInfo(Version version) {
            PublishedVersion = version;
            Version = version.ToString();
            LastSyncedAt = Tools.Generic.GetCurrentUtcDateTime;
        }

        public Uri GetAuthorUri() {
            var repo = CustomRepo;
            if (repo != null)
                return repo.GetInfoUri(ServerKey);
            return Author == null
                ? Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "me", "content")
                : Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "profile", Author);
        }

        public Uri GetPwsUri() {
            return new Uri("pws://?c=" + GetShortId());
        }

        public async Task ChangeScope(IConnectApiHandler api, CollectionScope desiredScope) {
            await api.ChangeCollectionScope(PublishedId.Value, desiredScope);
            PublishingScope = desiredScope;
            if (desiredScope == CollectionScope.Private)
                Subscribers = null;
        }

        public async Task UploadAvatar(IAbsoluteFilePath filePath, IConnectApiHandler api) {
            ImageLarge =
                Image = "http:" + await api.UploadCollectionAvatar(filePath, PublishedId.Value).ConfigureAwait(false);
        }

        public async Task GenerateNewAvatar(IConnectApiHandler api) {
            ImageLarge = Image = "http:" + await api.GenerateNewCollectionImage(PublishedId.Value).ConfigureAwait(false);
        }

        void UpdateRepositoriesFromCustomRepo() {
            Repositories.Clear();
            Repositories.Add(CustomRepoUrl.ToUri().ProcessRepoUrl());
        }

        public void SetRequired(ToggleableModProxy toggleableModProxy, bool b) {
            if (b)
                MakeRequired(toggleableModProxy);
            else
                MakeUnrequired(toggleableModProxy);
        }
    }

    public class CollectionImageUploadException : Exception
    {
        public CollectionImageUploadException(string message, Exception inner) : base(message, inner) {}
    }

    public class CollectionDescriptionMissingException : UserException
    {
        public CollectionDescriptionMissingException() : base(null) {}
    }

    public class CollectionNameMissingException : UserException
    {
        public CollectionNameMissingException() : base(null) {}
    }

    public class CollectionEmptyException : UserException
    {
        public CollectionEmptyException() : base(null) {}
    }
}