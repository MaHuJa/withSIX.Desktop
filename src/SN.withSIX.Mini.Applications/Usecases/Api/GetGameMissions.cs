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
    public class GetGameMissions : IAsyncQuery<GameMissionsApiModel>, IHaveId<Guid>
    {
        public GetGameMissions(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }


    public class GetGameMissionsHandler : DbQueryBase, IAsyncRequestHandler<GetGameMissions, GameMissionsApiModel>
    {
        public GetGameMissionsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) { }

        public async Task<GameMissionsApiModel> HandleAsync(GetGameMissions request)
        {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            return game.MapTo<GameMissionsApiModel>();
        }
    }

    public class GameMissionsApiModel {
        public List<ContentApiModel> Missions { get; set; }
    }
}