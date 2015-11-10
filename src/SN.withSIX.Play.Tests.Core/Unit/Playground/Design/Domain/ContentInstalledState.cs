// <copyright company="SIX Networks GmbH" file="ContentInstalledState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class ContentInstalledState
    {
        public ContentInstalledState(SpecificVersion version, IAbsoluteDirectoryPath directory) {
            Version = version;
            Directory = directory;
        }

        protected ContentInstalledState() {}

        public virtual bool IsInstalled {
            get { return true; }
        }

        public IAbsoluteDirectoryPath Directory { get; private set; }
        public SpecificVersion Version { get; private set; }
    }

    public class ContentNotInstalledState : ContentInstalledState
    {
        public override bool IsInstalled {
            get { return false; }
        }
    }
}