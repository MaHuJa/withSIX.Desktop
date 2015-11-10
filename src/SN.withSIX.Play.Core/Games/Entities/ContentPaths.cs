// <copyright company="SIX Networks GmbH" file="ContentPaths.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public class ContentPaths
    {
        public ContentPaths(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath repositoryPath) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(repositoryPath != null);

            Path = path;
            RepositoryPath = repositoryPath;
        }

        protected ContentPaths() {}
        public IAbsoluteDirectoryPath Path { get; }
        public IAbsoluteDirectoryPath RepositoryPath { get; }
        public virtual bool IsValid
        {
            get { return true; }
        }

        public bool EqualPath(ContentPaths other) {
            return Path.EqualsNullSupported(other.Path);
        }

        public bool EqualRepositoryPath(ContentPaths other) {
            return RepositoryPath.EqualsNullSupported(other.RepositoryPath);
        }
    }

    public class NullContentPaths : ContentPaths
    {
        public override bool IsValid
        {
            get { return false; }
        }
    }
}