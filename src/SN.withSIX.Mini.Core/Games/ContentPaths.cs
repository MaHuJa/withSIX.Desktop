// <copyright company="SIX Networks GmbH" file="ContentPaths.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;

namespace SN.withSIX.Mini.Core.Games
{
    public class ContentPaths
    {
        public static readonly ContentPaths Default = new NullContentPaths();

        public ContentPaths(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath repositoryPath) {
            /*
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(repositoryPath != null);
            */

            Path = path;
            RepositoryPath = repositoryPath;
        }

        protected ContentPaths() {}
        public IAbsoluteDirectoryPath Path { get; }
        public IAbsoluteDirectoryPath RepositoryPath { get; }
        public virtual bool IsValid => true;

        public bool EqualPath(ContentPaths other) {
            return Path.EqualsNullSupported(other.Path);
        }

        public bool EqualRepositoryPath(ContentPaths other) {
            return RepositoryPath.EqualsNullSupported(other.RepositoryPath);
        }

        class NullContentPaths : ContentPaths
        {
            public override bool IsValid => false;
        }
    }
}