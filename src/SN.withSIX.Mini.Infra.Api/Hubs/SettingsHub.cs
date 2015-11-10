// <copyright company="SIX Networks GmbH" file="SettingsHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Usecases.Api;

namespace SN.withSIX.Mini.Infra.Api.Hubs
{
    public class SettingsHub : HubBase<ISettingsClientHub>
    {
        public Task<GeneralSettings> GetGeneral() {
            return RequestAsync(new GetGeneralSettings());
        }

        public Task<GamesSettings> GetGames() {
            return RequestAsync(new GetGamesSettings());
        }

        public Task<GameSettings> GetGame(Guid id) {
            return RequestAsync(new GetGameSettings(id));
        }

        public Task SaveGeneralSettings(SaveGeneralSettings command) {
            return RequestAsync(command);
        }

        public Task SaveGameSettings(SaveGameSettings command) {
            return RequestAsync(command);
        }
    }

    public interface ISettingsClientHub {}
}