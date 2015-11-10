// <copyright company="SIX Networks GmbH" file="BasicGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class BasicGame : Game, ILaunchWith<IBasicGameLauncher>
    {
        protected BasicGame(Guid id, GameSettings settings) : base(id, settings) {}

        protected override Task InstallImpl(IContentInstallationService installationService,
            IContentAction<IInstallableContent> content) => installationService.Install(GetInstallAction(content));

        protected virtual InstallContentAction GetInstallAction(
            IContentAction<IInstallableContent> action)
            => new InstallContentAction(action.Content, action.CancelToken) {
                RemoteInfo = RemoteInfo,
                Paths = ContentPaths,
                Game = this,
                Cleaning = ContentCleaning
            };

        protected override Task UninstallImpl(IContentInstallationService contentInstallation,
            IContentAction<IUninstallableContent> uninstallLocalContentAction)
            => contentInstallation.Uninstall(GetUninstallAction(uninstallLocalContentAction));

        UnInstallContentAction GetUninstallAction(IContentAction<IUninstallableContent> action)
            => new UnInstallContentAction(this, action.Content, action.CancelToken) {
                Paths = ContentPaths
            };

        protected override async Task<Process> LaunchImpl(IGameLauncherFactory factory,
            ILaunchContentAction<IContent> launchContentAction) {
            var launcher = factory.Create(this);
            return await(IsLaunchingSteamApp()
                ? LaunchWithSteam(launcher, GetStartupParameters())
                : LaunchNormal(launcher, GetStartupParameters())).ConfigureAwait(false);
        }

        protected async Task<Process> LaunchNormal(ILaunch launcher, IEnumerable<string> startupParameters)
            =>
                await
                    launcher.Launch(await GetDefaultLaunchInfo(startupParameters).ConfigureAwait(false))
                        .ConfigureAwait(false);

        protected async Task<Process> LaunchWithSteam(ILaunchWithSteam launcher, IEnumerable<string> startupParameters)
            =>
                await
                    launcher.Launch(await GetSteamLaunchInfo(startupParameters).ConfigureAwait(false))
                        .ConfigureAwait(false);

        IEnumerable<string> GetStartupParameters() => Settings.StartupParameters.Get();
        // TODO
        protected override async Task ScanForLocalContentImpl() {}
    }
}