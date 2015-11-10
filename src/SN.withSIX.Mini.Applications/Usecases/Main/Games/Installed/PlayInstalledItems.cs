// <copyright company="SIX Networks GmbH" file="PlayInstalledItems.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed
{
    public class PlayInstalledItems : GameContentBase, IAsyncVoidCommand
    {
        public PlayInstalledItems(Guid gameId, List<Guid> contents) : base(gameId) {
            Ids = contents;
        }

        public List<Guid> Ids { get; }
    }

    public class PlayInstalledItemsHandler : DbCommandBase, IAsyncVoidCommandHandler<PlayInstalledItems>
    {
        readonly IContentInstallationService _contentInstallation;
        readonly IGameLauncherFactory _factory;

        public PlayInstalledItemsHandler(IDbContextLocator dbContextLocator, IGameLauncherFactory factory,
            IContentInstallationService contentInstallation) : base(dbContextLocator) {
            _factory = factory;
            _contentInstallation = contentInstallation;
        }

        // TODO: LocalContent doesnt need a spec??
        public async Task<UnitType> HandleAsync(PlayInstalledItems request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);

            // TODO: Optimize query
            using (var cts = new DoneCancellationTokenSource()) {
                var action =
                    new PlayLocalContentAction(
                        request.Ids.Select(x => new LocalContentSpec(game.LocalContent.FindOrThrow(x)))
                            .ToArray(), cancelToken: cts.Token);

                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Play(_factory, _contentInstallation, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}