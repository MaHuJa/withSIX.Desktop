// <copyright company="SIX Networks GmbH" file="InstallSynqPackageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public class InstallSynqPackageCommand : SynqPackageCommand, IAsyncRequest<UnitType>
    {
        public InstallSynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }

    public class InstallFileBasedSynqPackageCommand : InstallSynqPackageCommand
    {
        public InstallFileBasedSynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }
}