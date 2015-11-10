// <copyright company="SIX Networks GmbH" file="UninstallSynqPackageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public class UninstallSynqPackageCommand : SynqPackageCommand, IAsyncRequest<UnitType>
    {
        public UninstallSynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }

    public class UninstallFileBasedSynqPackageCommand : UninstallSynqPackageCommand
    {
        public UninstallFileBasedSynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }
}