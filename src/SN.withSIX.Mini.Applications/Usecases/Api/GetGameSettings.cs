// <copyright company="SIX Networks GmbH" file="GetGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGameSettings : IAsyncQuery<GameSettings>, IHaveId<Guid>
    {
        public GetGameSettings(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameSettingsHandler : DbQueryBase, IAsyncRequestHandler<GetGameSettings, GameSettings>
    {
        readonly IGameSettingsViewModelFactory _factory;
        public GetGameSettingsHandler(IDbContextLocator dbContextLocator, IGameSettingsViewModelFactory factory) : base(dbContextLocator) {
            _factory = factory;
        }

        public async Task<GameSettings> HandleAsync(GetGameSettings request) {
            return new GameSettings {
                Settings = _factory.CreateApiModel(
                    await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false))
            };
        }
    }

    public class GameSettings
    {
        public object Settings { get; set; }
    }
}