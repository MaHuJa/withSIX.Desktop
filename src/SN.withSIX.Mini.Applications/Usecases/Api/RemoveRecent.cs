using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class RemoveRecent : IAsyncVoidCommand, IHaveId<Guid>, IHaveGameId
    {
        public RemoveRecent(Guid gameId, Guid id) {
            GameId = gameId;
            Id = id;
        }

        public Guid Id { get; }
        public Guid GameId { get; }
    }

    public class RemoveRecentHandler : DbCommandBase, IAsyncVoidCommandHandler<RemoveRecent>
    {
        public RemoveRecentHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
        public async Task<UnitType> HandleAsync(RemoveRecent request) {
            await GameContext.Load(request.GameId).ConfigureAwait(false);
            var game = await GameContext.FindGameOrThrowAsync(request).ConfigureAwait(false);
            var content = await game.Contents.FindOrThrowFromRequestAsync(request).ConfigureAwait(false);
            content.RemoveRecentInfo();
            await GameContext.SaveChanges().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}