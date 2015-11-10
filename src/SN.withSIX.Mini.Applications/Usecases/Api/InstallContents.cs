// <copyright company="SIX Networks GmbH" file="InstallContents.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class InstallContents : ContentsBase, IAsyncVoidCommand
    {
        public InstallContents(Guid gameId, List<ContentGuidSpec> contents) : base(gameId, contents) {}
    }

    public class InstallContentsHandler : ApiDbCommandBase, IAsyncVoidCommandHandler<InstallContents>
    {
        readonly IContentInstallationService _contentInstallation;

        public InstallContentsHandler(IDbContextLocator dbContextLocator,
            IContentInstallationService contentInstallation)
            : base(dbContextLocator) {
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(InstallContents request) {
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            await RaiseGameEvent(game).ConfigureAwait(false);

            using (var cts = new DoneCancellationTokenSource()) {
                var action = GetAction(request, game, cts.Token);
                game.UseContent(action, cts);
                await GameContext.SaveChanges().ConfigureAwait(false);

                await game.Install(_contentInstallation, action).ConfigureAwait(false);
                await GameContext.SaveChanges().ConfigureAwait(false);
            }

            return UnitType.Default;
        }

        static DownloadContentAction GetAction(InstallContents request, Game game, CancellationToken token) {
            // TODO: Optimize query
            var action =
                new DownloadContentAction(token, content:
                    request.Contents.Select(x => new { Content = game.Contents.FindOrThrow(x.Id), x.Constraint })
                    .Where(x => x.Content is IInstallableContent)
                    .Select(
                        x => new InstallContentSpec((IInstallableContent) x.Content, x.Constraint))
                        .ToArray()) {Name = request.Name};
            return action;
        }
    }
}