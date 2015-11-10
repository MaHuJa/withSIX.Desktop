// <copyright company="SIX Networks GmbH" file="StatusHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Infra.Api.Messengers;

namespace SN.withSIX.Mini.Infra.Api.Hubs
{
    public class StatusHub : HubBase<IStatusClientHub>
    {
        public Task<ClientContentInfo> GetState(Guid gameId) {
            return RequestAsync(new GetState(gameId));
        }

        public Task<GamesApiModel> GetGames()
        {
            return RequestAsync(new GetGames());
        }

        public Task<HomeApiModel> GetHome() {
            return RequestAsync(new GetHome());
        }

        public Task<GameHomeApiModel> GetGameHome(Guid id) {
            return RequestAsync(new GetGameHome(id));
        }

        public Task<GameCollectionsApiModel> GetGameCollections(Guid id)
        {
            return RequestAsync(new GetGameCollections(id));
        }

        public Task<GameModsApiModel> GetGameMods(Guid id)
        {
            return RequestAsync(new GetGameMods(id));
        }

        public Task<GameMissionsApiModel> GetGameMissions(Guid id)
        {
            return RequestAsync(new GetGameMissions(id));
        }
    }

    // TODO: More advanced state handling events including if updates are available
    // the domain needs to be extended to support that kind of stuff.
    // for now handle uptodate as Installed state..

    public interface IStatusClientHub
    {
        Task LockedGame(Guid gameId);
        Task UnlockedGame(Guid gameId);
        Task ContentStateChanged(ContentStateChange state);
        Task ContentStatusChanged(ContentStatusChangedModel contentStatusChangedModel);
        Task StatusChanged(StatusModel notification);
        Task LaunchedGame(Guid id);
        Task TerminatedGame(Guid id);
    }
}