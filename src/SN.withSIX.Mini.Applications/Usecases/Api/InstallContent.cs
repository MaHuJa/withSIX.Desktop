// <copyright company="SIX Networks GmbH" file="InstallContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class InstallContent : SingleCntentBase, IAsyncVoidCommand
    {
        public InstallContent(Guid gameId, ContentGuidSpec content) : base(gameId, content) {}
    }

    public class InstallContentHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<InstallContent>
    {
        readonly IContentInstallationService _contentInstallation;

        public InstallContentHandler(IDbContextLocator dbContextLocator,
            IContentInstallationService contentInstallation)
            : base(dbContextLocator) {
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(InstallContent request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var content = game.Contents.First(x => x.Id == request.Content.Id);
            await RaiseGameEvent(game).ConfigureAwait(false);

            using (var cts = new DoneCancellationTokenSource()) {
                var action =
                    new DownloadContentAction(cts.Token, content: new InstallContentSpec((IInstallableContent) content));
                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);
                await game.Install(_contentInstallation, action).ConfigureAwait(false);

                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }
    }
}