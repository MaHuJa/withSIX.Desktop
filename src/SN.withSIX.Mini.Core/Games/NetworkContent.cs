// <copyright company="SIX Networks GmbH" file="NetworkContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Extensions;

namespace SN.withSIX.Mini.Core.Games
{
    public interface INetworkContent : IPackagedContent, IInstallableContent, IHaveImage, IHavePath
    {
        ICollection<NetworkContentSpec> Dependencies { get; }
    }

    public interface IHavePath
    {
        string ContentSlug { get; }
        string GetPath();
    }

    public interface IHaveImage
    {
        Uri Image { get; }
    }

    [DataContract]
    public abstract class NetworkContent : PackagedContent, INetworkContent
    {
        protected NetworkContent() {}
        protected NetworkContent(string name, string packageName, Guid gameId) : base(name, packageName, gameId) {}
        [DataMember]
        // Only here because of wrong dependency references... // TODO: Try to externalize this to the DTO instead..
        public ICollection<ContentGuidSpec> InternalDependencies { get; protected set; } = new List<ContentGuidSpec>();
        [DataMember]
        public List<string> Aliases { get; protected set; } = new List<string>();
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public DateTime UpdatedVersion { get; set; }
        [IgnoreDataMember]
        public virtual ICollection<NetworkContentSpec> Dependencies { get; } = new List<NetworkContentSpec>();

        public string GetPath() {
            return this.GetContentPath(ContentSlug);
        }

        public abstract string ContentSlug { get; }

        public override IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null) {
            if (list == null)
                list = new List<IContentSpec<Content>>();

            if (list.Select(x => x.Content).Contains(this))
                return list;

            var spec = new NetworkContentSpec(this, constraint);
            list.Add(spec);
            foreach (var d in Dependencies)
                d.Content.GetRelatedContent(list, d.Constraint);
            list.RemoveAll(x => x.Content == this);
            list.Add(spec);

            return list;
        }

        [OnSerialized]
        void OnSerialized(StreamingContext context) {
            InternalDependencies =
                Dependencies.Select(x => new ContentGuidSpec(x.Content.Id, x.Constraint)).ToList();
        }
    }
}