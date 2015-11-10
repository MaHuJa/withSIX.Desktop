// <copyright company="SIX Networks GmbH" file="VerifySynqPackageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public class VerifySynqPackageCommand : SynqPackageCommand, IAsyncRequest<UnitType>
    {
        public VerifySynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }

    public class VerifyFileBasedSynqPackageCommand : VerifySynqPackageCommand
    {
        public VerifyFileBasedSynqPackageCommand(PackageItem package, ContentPaths paths) : base(package, paths) {}
    }
}