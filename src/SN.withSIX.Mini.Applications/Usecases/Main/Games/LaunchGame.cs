// <copyright company="SIX Networks GmbH" file="LaunchGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    [ApiUserAction]
    public class LaunchGame : IAsyncVoidCommand, IHaveId<Guid>
    {
        public LaunchGame(Guid id, LaunchType launchType) {
            Id = id;
            LaunchType = launchType;
        }

        public LaunchType LaunchType { get; }
        public Guid Id { get; }
    }

    public class LaunchGameHandler : DbCommandBase, IAsyncVoidCommandHandler<LaunchGame>
    {
        readonly IGameLauncherFactory _launcherFactory;

        public LaunchGameHandler(IDbContextLocator dbContextLocator, IGameLauncherFactory launcherFactory)
            : base(dbContextLocator) {
            _launcherFactory = launcherFactory;
        }

        public async Task<UnitType> HandleAsync(LaunchGame request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);

            var launchContentAction = new LaunchContentAction(request.LaunchType);
            await game.Launch(_launcherFactory, launchContentAction).ConfigureAwait(false);

            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}