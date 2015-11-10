// <copyright company="SIX Networks GmbH" file="ContentSpec.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Core.Games
{
    public interface IContentSpec<out T> where T : IContent
    {
        T Content { get; }
        string Constraint { get; }
    }

    [DataContract]
    public class ContentSpec<T> : IEquatable<ContentSpec<T>>, IContentSpec<T> where T : IContent
    {
        public ContentSpec(T content, string constraint = null) {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            Content = content;
            Constraint = constraint;
        }

        [DataMember]
        public T Content { get; }
        [DataMember]
        public string Constraint { get; }
        // TODO: Consider if ContentSpecs are equal even if Constraint doesn't matches, but Content matches.
        // As we only want a single entry even when Constraints differ, one might lean towards only checking the Content object??
        // If we keep the current implementation then:
        // - We can have the same Content multiple times processed
        // - JSON.NET does not properly detect self referencing problems

        // TODO: This would also be a problem for Entity Framework, etc, or not?
        public bool Equals(ContentSpec<T> other) {
            return other?.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(Content)*397) ^ (Constraint?.GetHashCode() ?? 0);
            }
        }

        public override bool Equals(object other) {
            return Equals(other as ContentSpec<T>);
        }
    }

    [DataContract]
    public class ContentSpec : ContentSpec<Content>
    {
        public ContentSpec(Content content, string constraint = null) : base(content, constraint) {}
    }

    [DataContract]
    public class LocalContentSpec : ContentSpec<LocalContent>
    {
        public LocalContentSpec(LocalContent content, string constraint = null) : base(content, constraint) {}
    }

    [DataContract]
    public class NetworkContentSpec : ContentSpec<NetworkContent>
    {
        public NetworkContentSpec(NetworkContent content, string constraint = null) : base(content, constraint) {}
    }

    [DataContract]
    public class NetworkContentRelation : NetworkContentSpec
    {
        public NetworkContentRelation(NetworkContent content, NetworkContent self, string constraint = null)
            : base(content, constraint) {
            Self = self;
        }

        public NetworkContent Self { get; }
    }

    [DataContract]
    public class CollectionContentSpec : ContentSpec<Collection>
    {
        public CollectionContentSpec(Collection content, string constraint = null) : base(content, constraint) {}
    }

    [DataContract]
    public class PackagedContentSpec : ContentSpec<PackagedContent>
    {
        public PackagedContentSpec(PackagedContent content, string constraint = null) : base(content, constraint) {}
    }

    [DataContract]
    public class InstallContentSpec : ContentSpec<IInstallableContent>
    {
        public InstallContentSpec(IInstallableContent content, string constraint = null) : base(content, constraint) {}
    }


    public interface IContentIdSpec<out T> : IHaveId<T>
    {
        string Constraint { get; }
    }

    [DataContract]
    public abstract class ContentIdSpec<T> : IContentIdSpec<T>
    {
        protected ContentIdSpec(T id, string constraint = null) {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Id = id;
            Constraint = constraint;
        }

        [DataMember]
        public string Constraint { get; }
        [DataMember]
        public T Id { get; }
    }

    [DataContract]
    public class ContentGuidSpec : ContentIdSpec<Guid>
    {
        public ContentGuidSpec(Guid id, string constraint = null) : base(id, constraint) {}
    }
}