// <copyright company="SIX Networks GmbH" file="GetHome.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetHome : IAsyncQuery<HomeApiModel> {}

    public class GetHomeHandler : DbQueryBase, IAsyncRequestHandler<GetHome, HomeApiModel>
    {
        public GetHomeHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<HomeApiModel> HandleAsync(GetHome request) {
            await GameContext.LoadAll().ConfigureAwait(false);
            var games = await GameContext.Games.Where(x => x.InstalledState.IsInstalled).ToListAsync().ConfigureAwait(false);

            return games.MapTo<HomeApiModel>();
        }
    }

    public class HomeApiModel
    {
        public List<GameApiModel> Games { get; set; }
        public List<ContentApiModel> NewContent { get; set; }
        public List<ContentApiModel> Updates { get; set; }
        public List<ContentApiModel> Recent { get; set; }
    }

    public class ContentApiModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
        public string Slug { get; set; }
        public string Type { get; set; }
        public string GameSlug { get; set; }
        public Guid GameId { get; set; }
        public string Author { get; set; }
        public Uri Image { get; set; }
        public string Version { get; set; }
        public bool IsFavorite { get; set; }
        public TypeScope TypeScope { get; set; }
    }

    public enum TypeScope
    {
        Local,
        Subscribed,
        Published
    }

    public abstract class GameApiModelBase
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public Uri Image { get; set; }
    }

    public class GameApiModel : GameApiModelBase
    {
        public int CollectionsCount { get; set; }
        public int MissionsCount { get; set; }
        public int ModsCount { get; set; }
    }
}