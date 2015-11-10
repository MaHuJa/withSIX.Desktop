// <copyright company="SIX Networks GmbH" file="SynqPackageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public abstract class SynqPackageCommand
    {
        protected SynqPackageCommand(PackageItem package, ContentPaths paths) {
            Contract.Requires<ArgumentNullException>(package != null);
            Contract.Requires<ArgumentNullException>(paths != null);

            Package = package;
            Paths = paths;
        }

        public PackageItem Package { get; private set; }
        public ContentPaths Paths { get; private set; }
    }
}