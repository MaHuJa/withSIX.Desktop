// <copyright company="SIX Networks GmbH" file="AdvancedCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DataContract(Name = "AdvancedCollection",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class AdvancedCollection : Collection
    {
        [DataMember] List<Uri> _repositories = new List<Uri>();
        [DataMember] List<CollectionServer> _servers = new List<CollectionServer>();
        protected AdvancedCollection(Guid id, ISupportModding game) : base(id, game) {}
        protected AdvancedCollection(Guid id) : base(id) {}
        public IReadOnlyCollection<IMod> CustomRepoMods { get; set; }
        public List<Uri> Repositories
        {
            get { return _repositories; }
            protected set { _repositories = value; }
        }
        public List<CollectionServer> Servers
        {
            get { return _servers; }
            protected set { _servers = value; }
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext ctx) {
            if (_repositories == null)
                _repositories = new List<Uri>();
            if (_servers == null)
                _servers = new List<CollectionServer>();
        }

        protected override void HandleModsetMods(IContentManager modList) {
            HandleModsetModsInternal(modList, CustomRepoMods);
        }

        public virtual async Task HandleCustomRepositories(IContentManager manager, bool report) {
            var repos = await Task.WhenAll(Repositories.Select(x => GetRepo(manager, x))).ConfigureAwait(false);
            CustomRepoMods = repos.SelectMany(x => x.Mods).ToArray();
            await ProcessServers(manager).ConfigureAwait(false);
            HandleModsetMods(manager);
            UpdateState();
        }

        protected override IEnumerable<IMod> ConvertToMods(CollectionVersionModel collectionVersion,
            IContentManager contentList) {
            return contentList.FindOrCreateLocalMods(Game,
                collectionVersion.Dependencies.Select(x => x.Dependency), CustomRepoMods);
        }

        protected override void HandleDependencies(IContentManager modList) {
            string[] mods;
            lock (Items)
                mods = ModItems().Select(x => x.GetSerializationString()).ToArray();

            var dependencies = modList.GetDependencies(mods,
                CustomRepoMods);
            Dependencies = modList.FindOrCreateLocalMods(Game, dependencies, CustomRepoMods)
                .ToArray();
        }

        protected void UpdateServersInfo(CollectionVersionModel collectionVersion) {
            if (collectionVersion.Servers != null) {
                Servers =
                    collectionVersion.Servers.Select(
                        x => new CollectionServer {Address = new ServerAddress(x.Address), Password = x.Password})
                        .ToList();
            } else
                Servers.Clear();
        }

        protected override async Task SynchronizeMods(IContentManager contentList,
            CollectionVersionModel collectionVersion) {
            await HandleCustomRepositories(contentList, false).ConfigureAwait(false);
            await base.SynchronizeMods(contentList, collectionVersion).ConfigureAwait(false);
        }

        async Task ProcessServers(IContentManager manager) {
            if (!Servers.Any())
                return;
            var s = Servers.First();
            var server = manager.ServerList.FindOrCreateServer(new ServerAddress(s.Address.IP, s.Address.Port + 1), true);
            if (s.Password != null)
                server.SavedPassword = s.Password;
            await server.TryUpdateAsync().ConfigureAwait(false);
            ((Game) Game).CalculatedSettings.Server = server;
        }

        protected static Task<SixRepo> GetRepo(IContentManager manager, Uri uri) {
            var repo = Repo.Create(uri.ProcessRepoUrl());
            switch (repo.Type) {
            case RepoType.SixSync: {
                return manager.GetRepo(repo.Uri);
            }
            default: {
                throw new NotSupportedException("The specified repo type is not supported: " + repo.Type + ", " + uri);
            }
            }
        }

        protected class Repo
        {
            Repo(Uri uri, RepoType type) {
                Contract.Requires<ArgumentNullException>(uri != null);
                Uri = uri;
                Type = type;
            }

            public Uri Uri { get; }
            public RepoType Type { get; }

            public static Repo Create(Uri uri) {
                var type = GetTypeFromUri(uri);
                var protocol = GetProtocolFromUri(uri);
                return new Repo(ReplaceProtocol(uri, protocol), type);
            }

            static Uri ReplaceProtocol(Uri uri, string protocol) {
                var uriBuilder = new UriBuilder(uri) {Scheme = protocol};
                return uriBuilder.Uri;
            }

            static string GetProtocolFromUri(Uri uri) {
                if (uri.Scheme == "sixsyncs" || uri.Scheme == "synqs")
                    return "https";
                return "http";
            }

            static RepoType GetTypeFromUri(Uri uri) {
                if (uri.Scheme == "synq" || uri.Scheme == "synq")
                    return RepoType.Synq;
                return RepoType.SixSync;
            }
        }

        protected enum RepoType
        {
            SixSync,
            Synq
        }
    }
}