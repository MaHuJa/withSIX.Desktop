// <copyright company="SIX Networks GmbH" file="RealVirtualityMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public class RealVirtualityLaunchGlobalState : LaunchableSomething<RealVirtualityLaunchState> {}

    public class RealVirtualityLaunchState {}

    public class RealVirtualityMod<TGameData> : Mod<TGameData, RealVirtualityLaunchGlobalState>
        where TGameData : RealVirtualityGameData, IModdingGameData
    {
        public RealVirtualityMod(Guid id, PackageItem package, ModMetaData metaData) : base(id, package, metaData) {}

        protected override ContentInstalledState GetInstalledState(TGameData gameData) {
            var installDir = GetInstallDirectory(gameData);
            return installDir.Exists
                ? new ContentInstalledState(
                    SN.withSIX.Sync.Core.Packages.Package.ReadSynqInfoFile(installDir), installDir)
                : new ContentNotInstalledState();
        }

        public override void MakeLaunchState(RealVirtualityLaunchGlobalState sharedLaunchState,
            IMediator domainMediator) {
            throw new NotImplementedException();
        }

        public override async Task Install(TGameData gameData, IMediator mediator) {
            await base.Install(gameData, mediator).ConfigureAwait(false);
            await
                mediator.RequestAsync(
                    new InstallUserconfigCommand(GetInstallDirectory(gameData), gameData.Directory))
                    .ConfigureAwait(false);
            await
                mediator.RequestAsync(new InstallTeamspeakPluginCommand(GetInstallDirectory(gameData)))
                    .ConfigureAwait(false);
        }

        IAbsoluteDirectoryPath GetInstallDirectory(IModdingGameData gameData) {
            return gameData.ModPaths.Path.GetChildDirectoryWithName(MetaData.Name);
        }

        protected override void DeleteAsync() {
            throw new NotImplementedException();
        }
    }
}