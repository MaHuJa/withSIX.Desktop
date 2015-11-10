// <copyright company="SIX Networks GmbH" file="GetRecent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class GetRecent : IAsyncQuery<IRecentViewModel>, IHaveId<Guid>
    {
        public GetRecent(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetRecentHandler : DbQueryBase, IAsyncRequestHandler<GetRecent, IRecentViewModel>
    {
        public GetRecentHandler(IDbContextLocator dbContextLocator)
            : base(dbContextLocator) {}

        public async Task<IRecentViewModel> HandleAsync(GetRecent request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<RecentViewModel>();
        }
    }
}