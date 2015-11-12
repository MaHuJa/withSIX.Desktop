// <copyright company="SIX Networks GmbH" file="GetGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Factories;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Applications.Usecases.Settings
{
    public class GetGameSettings : IAsyncQuery<IGameSettingsTabViewModel>, IHaveId<Guid>
    {
        public GetGameSettings(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetGameSettingsHandler : DbQueryBase,
        IAsyncRequestHandler<GetGameSettings, IGameSettingsTabViewModel>
    {
        readonly IGameSettingsViewModelFactory _factory;

        public GetGameSettingsHandler(IDbContextLocator contextLocator, IGameSettingsViewModelFactory factory)
            : base(contextLocator) {
            _factory = factory;
        }

        public async Task<IGameSettingsTabViewModel> HandleAsync(GetGameSettings request) {
            return
                _factory.CreateViewModel(
                    await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false));
        }
    }
}