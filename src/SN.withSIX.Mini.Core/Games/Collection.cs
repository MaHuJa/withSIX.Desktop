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
        public CollectionInstalled(Guid gameId, Guid contentId) {
            GameId = gameId;
            ContentId = contentId;
        }

        public Guid GameId { get; set; }
        public Guid ContentId { get; set; }
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

        /*
        public override async Task PostInstall(IInstallerSession installerSession, CancellationToken cancelToken) {
            await base.PostInstall(installerSession, cancelToken);
            PrepareEvent(new CollectionInstalled(this.GameId, this.Id));
        }
*/

        public virtual string ContentSlug { get; } = "collections";
        [DataMember]
        public virtual ICollection<string> Repositories { get; protected set; } = new List<string>();
        [DataMember]
        public virtual ICollection<CollectionServer> Servers { get; protected set; } = new List<CollectionServer>();

        public override async Task Install(IInstallerSession installerSession, CancellationToken cancelToken,
            string constraint = null) {
            // TODO: Access custom repositories and include content
            // TODO: Expand the InstallerSession to understand and support SixSync custom repo mods...
            // TODO: Also do this for the Launch action on the game somehow...
            await installerSession.Install(GetPackaged(constraint)).ConfigureAwait(false);
            PrepareEvent(new CollectionInstalled(GameId, Id));
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