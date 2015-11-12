// <copyright company="SIX Networks GmbH" file="Collection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Extensions;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class Collection : InstallableContent
    {
        protected Collection() {}
        protected Collection(string name, Guid gameId) : base(name, gameId) {}
        // TODO: Handle circular?
        [DataMember]
        public virtual ICollection<CollectionContentSpec> Dependencies { get; protected set; } =
            new List<CollectionContentSpec>();
        // TODO: Client vs Server vs All ?
        // TODO: Optional vs Required ?
        [DataMember]
        public virtual ICollection<ContentSpec> Contents { get; protected set; } =
            new List<ContentSpec>();

        protected IEnumerable<IContentSpec<Collection>> GetCollections(string constraint = null)
            => GetRelatedContent(constraint: constraint).OfType<IContentSpec<Collection>>();

        public override async Task Install(IInstallerSession installerSession, CancellationToken cancelToken, string constraint = null) {
            await base.Install(installerSession, cancelToken, constraint).ConfigureAwait(false);
            foreach (var c in GetCollections(constraint))
                await c.Content.PostInstall(installerSession, cancelToken).ConfigureAwait(false);
        }

        public override IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null) {
            if (list == null)
                list = new List<IContentSpec<Content>>();

            if (list.Select(x => x.Content).Contains(this))
                return list;

            var spec = new CollectionContentSpec(this, constraint);
            list.Add(spec);

            foreach (var d in Dependencies)
                d.Content.GetRelatedContent(list, d.Constraint);

            foreach (var c in Contents)
                c.Content.GetRelatedContent(list, c.Constraint);

            list.RemoveAll(x => x.Content == this);
            list.Add(spec);

            return list;
        }

        public override IEnumerable<string> GetContentNames() {
            return Contents.Select(x => x.Content.Name);
        }
    }

    public class CollectionInstalled : IDomainEvent
    {
        public CollectionInstalled(Guid gameId, Guid contentId, string version) {
            GameId = gameId;
            ContentId = contentId;
            Version = version;
        }

        public Guid GameId { get; }
        public Guid ContentId { get; }
        public string Version { get; }
    }

    [DataContract]
    public class LocalCollection : Collection
    {
        public LocalCollection(Guid gameId, string name, ICollection<ContentSpec> contents) : base(name, gameId) {
            Contents = contents;
        }
    }

    [DataContract]
    public abstract class NetworkCollection : Collection, IHaveRepositories, IHaveServers, ILaunchableContent, IHavePath
        // Hmm ILaunchableContent.. (is to allow SErvers to be collected from this collection, not sure if best)
    {
        protected NetworkCollection(Guid id, string name, Guid gameId) {
            Id = id;
            Name = name;
            GameId = gameId;
        }

        public string GetPath() {
            return this.GetContentPath(ContentSlug);
        }

        [DataMember]
        public string Version { get; protected set; }

        public virtual string ContentSlug { get; } = "collections";
        [DataMember]
        public virtual ICollection<string> Repositories { get; protected set; } = new List<string>();
        [DataMember]
        public virtual ICollection<CollectionServer> Servers { get; protected set; } = new List<CollectionServer>();

        public override async Task PostInstall(IInstallerSession installerSession, CancellationToken cancelToken) {
            await base.PostInstall(installerSession, cancelToken).ConfigureAwait(false);
            PrepareEvent(new CollectionInstalled(GameId, Id, Version));
        }
    }

    [DataContract]
    public class SubscribedCollection : NetworkCollection
    {
        public SubscribedCollection(Guid id, string name, Guid gameId) : base(id, name, gameId) {}
    }

    [DataContract]
    public class CollectionServer
    {
        [DataMember]
        // ServerAddress
        public string Address { get; set; }
        [DataMember]
        public string Password { get; set; }
    }

    public interface IHaveServers
    {
        ICollection<CollectionServer> Servers { get; }
    }

    public interface IHaveRepositories
    {
        ICollection<string> Repositories { get; }
    }
}