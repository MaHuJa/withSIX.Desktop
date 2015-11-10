// <copyright company="SIX Networks GmbH" file="Content.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Core.Games
{
    public interface IContent : IHaveGameId
    {
        string Name { get; set; }
        Guid Id { get; }
        bool IsFavorite { get; set; }
        IEnumerable<ILaunchableContent> GetLaunchables(string constraint = null);
        Task PostInstall(IInstallerSession installerSession, CancellationToken cancelToken);
        void RegisterAdditionalPostInstallTask(Func<Task> task);
    }

    public interface IHavePackageName
    {
        string PackageName { get; set; }
    }

    public interface IPackagedContent : IContent, IHavePackageName {}

    public interface IModContent : IPackagedContent, ILaunchableContent {}

    public interface IMissionContent : IPackagedContent, ILaunchableContent {}

    public interface IInstallableContent : IContent
    {
        Task Install(IInstallerSession installerSession, CancellationToken cancelToken, string constraint = null);

        IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null);
    }

    public interface IUninstallableContent : IContent, IPackagedContent
    {
        Task Uninstall(IUninstallSession contentInstaller);
    }

    [DataContract]
    public abstract class Content : BaseEntityGuidId, IContent
    {
        protected Content() {}

        protected Content(string name, Guid gameId) : this() {
            Name = name;
            GameId = gameId;
        }

        [DataMember]
        public Uri Image { get; set; }
        [DataMember]
        public RecentInfo RecentInfo { get; set; }
        [DataMember]
        public string Author { get; set; }
        [IgnoreDataMember]
        List<Func<Task>> AdditionalPostInstallActions { get; } = new List<Func<Task>>();
        [DataMember]
        public Guid GameId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsFavorite { get; set; }

        public IEnumerable<ILaunchableContent> GetLaunchables(string constraint = null)
            => GetRelatedContent(constraint: constraint).Select(x => x.Content).OfType<ILaunchableContent>();

        public virtual async Task PostInstall(IInstallerSession installerSession, CancellationToken cancelToken) {
            foreach (var a in AdditionalPostInstallActions)
                await a().ConfigureAwait(false);
        }

        public void RegisterAdditionalPostInstallTask(Func<Task> task) {
            AdditionalPostInstallActions.Add(task);
        }

        public virtual IEnumerable<string> GetContentNames() {
            yield return Name;
        }

        public abstract IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null);

        public void MakeFavorite() {
            IsFavorite = true;
            PrepareEvent(new ContentFavorited(this));
        }

        public void Unfavorite() {
            IsFavorite = false;
            PrepareEvent(new ContentUnFavorited(this));
        }

        public void Use(LaunchType launchType = LaunchType.Default, DoneCancellationTokenSource cts = null) {
            RecentInfo = new RecentInfo(launchType);
            PrepareEvent(new ContentUsed(this, cts));
        }

        public void RemoveRecentInfo() {
            RecentInfo = null;
            PrepareEvent(new RecentItemRemoved(this));
        }
    }

    public class RecentItemRemoved : IDomainEvent
    {
        public Content Content { get; }

        public RecentItemRemoved(Content content) {
            Content = content;
        }
    }

    public class ContentFavorited : IDomainEvent
    {
        public ContentFavorited(Content content) {
            Content = content;
        }

        public Content Content { get; }
    }

    public class ContentUnFavorited : IDomainEvent
    {
        public ContentUnFavorited(Content content) {
            Content = content;
        }

        public Content Content { get; }
    }

    /*
    public class LocalContentFavorited : IDomainEvent
    {
        public LocalContentFavorited(LocalContent content) {
            Content = content;
        }

        public LocalContent Content { get; }
    }

    public class LocalContentUnFavorited : IDomainEvent
    {
        public LocalContentUnFavorited(LocalContent content) {
            Content = content;
        }

        public LocalContent Content { get; }
    }
    */

    public interface ILaunchableContent {}

    [DataContract]
    public abstract class InstallableContent : Content, IInstallableContent
    {
        protected InstallableContent(string name, Guid gameId) : base(name, gameId) {}
        protected InstallableContent() {}
        // TODO: We only call Install on top-level entities, like a collection, or like the top of a dependency tree
        // PostInstall is however called for every processed entity now...
        public virtual Task Install(IInstallerSession installerSession, CancellationToken cancelToken,
            string constraint = null) => installerSession.Install(GetPackaged(constraint));

        protected IEnumerable<IContentSpec<IPackagedContent>> GetPackaged(string constraint = null)
            => GetRelatedContent(constraint: constraint)
                .OfType<IContentSpec<IPackagedContent>>();
    }
}