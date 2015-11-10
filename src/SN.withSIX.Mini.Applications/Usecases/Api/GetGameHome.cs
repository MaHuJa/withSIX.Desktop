// <copyright company="SIX Networks GmbH" file="GetGameHome.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGameHome : IAsyncQuery<GameHomeApiModel>, IHaveId<Guid>
    {
        public GetGameHome(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameHomeHandler : DbQueryBase, IAsyncRequestHandler<GetGameHome, GameHomeApiModel>
    {
        public GetGameHomeHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GameHomeApiModel> HandleAsync(GetGameHome request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<GameHomeApiModel>();
        }
    }

    public class GameHomeApiModel : GameApiModelBase
    {
        public List<ContentApiModel> Recent { get; set; }
        public List<ContentApiModel> NewContent { get; set; }
        public List<ContentApiModel> Updates { get; set; }
    }
}