// <copyright company="SIX Networks GmbH" file="GetGamesSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGamesSettings : IAsyncQuery<GamesSettings> {}

    public class GetGamesSettingsHandler : DbQueryBase, IAsyncRequestHandler<GetGamesSettings, GamesSettings>
    {
        public GetGamesSettingsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GamesSettings> HandleAsync(GetGamesSettings request) {
            await GameContext.LoadAll().ConfigureAwait(false);
            return new GamesSettings {
                Games =
                    GameContext.Games
                    .Where(x => (Consts.Features.UnreleasedGames || x.Metadata.IsPublic))
                    .Select(
                        x => new GameSettingEntry {Id = x.Id, Slug = x.Metadata.Slug, Name = x.Metadata.Name}).ToList()
            };
        }
    }

    public class GamesSettings
    {
        public List<GameSettingEntry> Games { get; set; }
    }

    public class GameSettingEntry
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
    }
}