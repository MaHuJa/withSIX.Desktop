// <copyright company="SIX Networks GmbH" file="PlayInstalledItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed
{
    public class PlayInstalledItem : SingleCntentBase, IAsyncVoidCommand
    {
        public PlayInstalledItem(Guid gameId, ContentGuidSpec content) : base(gameId, content) {}
    }

    public class PlayInstalledItemHandler : DbCommandBase, IAsyncVoidCommandHandler<PlayInstalledItem>
    {
        readonly IContentInstallationService _contentInstallation;
        readonly IGameLauncherFactory _factory;

        public PlayInstalledItemHandler(IDbContextLocator dbContextLocator, IGameLauncherFactory factory,
            IContentInstallationService contentInstallation) : base(dbContextLocator) {
            _factory = factory;
            _contentInstallation = contentInstallation;
        }

        // TODO: LocalContent doesnt need a spec??
        public async Task<UnitType> HandleAsync(PlayInstalledItem request) {
            var game =
                await
                    GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var localContent = game.LocalContent.First(x => x.Id == request.Content.Id);

            using (var cts = new DoneCancellationTokenSource()) {
                var action = new PlayLocalContentAction(cancelToken: cts.Token,
                    content: new LocalContentSpec(localContent));
                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Play(_factory, _contentInstallation, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}