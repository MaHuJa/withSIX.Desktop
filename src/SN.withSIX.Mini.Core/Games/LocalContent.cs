// <copyright company="SIX Networks GmbH" file="LocalContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class LocalContent : Content, IHaveImage, IUninstallableContent, IHavePath, IHavePackageName
    {
        protected LocalContent() {}

        protected LocalContent(string name, string packageName, Guid gameId, string version) : base(name, gameId) {
            PackageName = packageName;
            Version = version;
        }

        // TODO: Should embed the network content instead... like with RecentItem?
        protected LocalContent(NetworkContent content, string version) : this() {
            UpdateFrom(content);
            if (version != null)
                Version = version;
        }

        [DataMember]
        public DateTime UpdatedVersion { get; set; }
        [DataMember]
        public InstallInfo InstallInfo { get; set; } = new InstallInfo();
        [DataMember]
        public NetworkContentSpec Content { get; protected set; }
        [DataMember]
        public Guid ContentId { get; protected set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public string Version { get; protected set; }
        [DataMember]
        public string Path { get; protected set; }

        public string GetPath() {
            return Path;
        }

        public void ChangeVersion(string version) {
            Version = version;
            InstallInfo.Updated();
        }

        [DataMember]
        public string ContentSlug { get; protected set; }
        [DataMember]
        public string PackageName { get; set; }

        public Task Uninstall(IUninstallSession installerSession) {
            return installerSession.Uninstall(this);
        }

        public void UpdateFrom(NetworkContent content) {
            Content = new NetworkContentSpec(content, Version);
            ContentId = content.Id;
            Name = content.Name;
            PackageName = content.PackageName;
            Image = content.Image;
            GameId = content.GameId;
            Author = content.Author;
            Path = content.GetPath();
            ContentSlug = content.ContentSlug;
            Version = content.Version;
            UpdatedVersion = content.UpdatedVersion;
        }

        public override IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null) {
            return Content != null ? Content.Content.GetRelatedContent(list, constraint) : HandleLocal(list, constraint);
        }

        protected virtual IEnumerable<IContentSpec<Content>> HandleLocal(List<IContentSpec<Content>> list,
            string constraint) {
            if (list == null)
                list = new List<IContentSpec<Content>>();

            if (list.Select(x => x.Content).Contains(this))
                return list;

            var spec = new LocalContentSpec(this, constraint);
            list.Add(spec);

            return list;
        }
    }

    [DataContract]
    public class ModLocalContent : LocalContent, IModContent
    {
        protected ModLocalContent() {}

        public ModLocalContent(string name, string packageName, Guid gameId, string version) : base(name, packageName, gameId, version) {
            ContentSlug = "mods";
        }

        public ModLocalContent(ModNetworkContent content, string version) : base(content, version) {
        }
    }

    [DataContract]
    public class ModRepoContent : ModLocalContent
    {
        protected ModRepoContent() {}
        public ModRepoContent(string name, string packageName, Guid gameId, string version) : base(name, packageName, gameId, version) {}
        [DataMember]
        // TODO: Actually build dependencies out of objects instead of strings
        public List<string> Dependencies { get; set; } = new List<string>();

        protected override IEnumerable<IContentSpec<Content>> HandleLocal(List<IContentSpec<Content>> list,
            string constraint) {
            if (list == null)
                list = new List<IContentSpec<Content>>();

            if (list.Select(x => x.Content).Contains(this))
                return list;

            var spec = new LocalContentSpec(this, constraint);
            list.Add(spec);
            // TODO: Dependencies of dependencies
            list.AddRange(Dependencies.Select(d => new LocalContentSpec(new ModLocalContent(d, d.ToLower(), GameId, null))));
            list.RemoveAll(x => x.Content == this);
            list.Add(spec);


            return list;
        }
    }

    [DataContract]
    public class MissionLocalContent : LocalContent, IMissionContent
    {
        protected MissionLocalContent() {}

        public MissionLocalContent(string name, string packageName, Guid gameId, string version) : base(name, packageName, gameId, version) {
            ContentSlug = "missions";
        }

        public MissionLocalContent(MissionNetworkContent content, string version) : base(content, version) {}
    }
}