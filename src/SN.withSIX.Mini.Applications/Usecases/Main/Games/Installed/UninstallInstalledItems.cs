// <copyright company="SIX Networks GmbH" file="UninstallInstalledItems.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed
{
    public class UninstallInstalledItems : GameContentBase, IAsyncVoidCommand
    {
        public UninstallInstalledItems(Guid gameId, List<Guid> contents) : base(gameId) {
            Ids = contents;
        }

        public List<Guid> Ids { get; }
    }

    public class UninstallInstalledItemsHandler : DbCommandBase, IAsyncVoidCommandHandler<UninstallInstalledItems>
    {
        readonly IContentInstallationService _contentInstallation;

        public UninstallInstalledItemsHandler(IDbContextLocator dbContextLocator,
            IContentInstallationService contentInstallation) : base(dbContextLocator) {
            _contentInstallation = contentInstallation;
        }

        // TODO: LocalContent doesnt need a spec??
        public async Task<UnitType> HandleAsync(UninstallInstalledItems request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);

            // TODO: Optimize query
            using (var cts = new DoneCancellationTokenSource()) {
                var action =
                    new UninstallLocalContentAction(
                        request.Ids.Select(x => new LocalContentSpec(game.LocalContent.FindOrThrow(x)))
                            .ToArray(), cancelToken: cts.Token);

                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Uninstall(_contentInstallation, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}