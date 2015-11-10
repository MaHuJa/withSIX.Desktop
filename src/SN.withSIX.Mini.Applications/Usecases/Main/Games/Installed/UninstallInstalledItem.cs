// <copyright company="SIX Networks GmbH" file="UninstallInstalledItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed
{
    [ApiUserAction]
    public class UninstallInstalledItem : IAsyncVoidCommand, IHaveGameId
    {
        public UninstallInstalledItem(Guid gameId, ContentGuidSpec content) {
            GameId = gameId;
            Content = content;
        }

        public ContentGuidSpec Content { get; }
        public Guid GameId { get; }
    }

    public class UninstallInstalledItemHandler : DbCommandBase, IAsyncVoidCommandHandler<UninstallInstalledItem>
    {
        readonly IContentInstallationService _contentInstallation;

        public UninstallInstalledItemHandler(IDbContextLocator dbContextLocator,
            IContentInstallationService contentInstallation) : base(dbContextLocator) {
            _contentInstallation = contentInstallation;
        }

        // TODO: LocalContent doesnt need a spec??
        public async Task<UnitType> HandleAsync(UninstallInstalledItem request) {
            var game =
                await
                    GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var localContent = game.LocalContent.First(x => x.ContentId == request.Content.Id || x.Id == request.Content.Id);

            var uninstallLocalContentAction =
                new UninstallLocalContentAction(content: new LocalContentSpec(localContent));

            await game.Uninstall(_contentInstallation, uninstallLocalContentAction).ConfigureAwait(false);

            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}