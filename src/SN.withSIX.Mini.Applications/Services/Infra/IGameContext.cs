// <copyright company="SIX Networks GmbH" file="IGameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface IGameContextReadOnly
    {
        ICollection<Game> Games { get; }

        [Obsolete("Only here because of half-assed custom JSON ORM :)")]
        Task Load(Guid gameId);

        [Obsolete("Only here because of half-assed custom JSON ORM :)")]
        Task LoadAll(bool skip = false);
    }

    public interface IGameContext : IGameContextReadOnly, IUnitOfWork, IDbContext
    {
        Task Migrate();
    }

    public static class GameContextExtensions
    {
        public static async Task<Game> FindGameOrThrowAsync(this IGameContextReadOnly gc, Guid id) {
            await gc.Load(id).ConfigureAwait(false);
            return await gc.Games.FindOrThrowAsync(id).ConfigureAwait(false);
        }

        public static async Task<Game> FindGameFromRequestOrThrowAsync(this IGameContextReadOnly gc,
            IHaveId<Guid> request) {
            await gc.Load(request.Id).ConfigureAwait(false);
            return await gc.Games.FindOrThrowFromRequestAsync(request).ConfigureAwait(false);
        }

        public static async Task<Game> FindGameOrThrowAsync(this IGameContextReadOnly gc, IHaveGameId request) {
            await gc.Load(request.GameId).ConfigureAwait(false);
            return await gc.Games.FindOrThrowAsync(request.GameId).ConfigureAwait(false);
        }

        public static Game FindGame(this IGameContextReadOnly gc, Guid id) {
            gc.Load(id).Wait();
            return gc.Games.Find(id);
        }
    }
}