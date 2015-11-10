// <copyright company="SIX Networks GmbH" file="SaveGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using AutoMapper;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class SaveGameSettings : IAsyncVoidCommand, IHaveId<Guid>
    {
        public object Settings { get; set; }
        public Guid Id { get; set; }
    }

    public class SaveGameSettingsHandler : DbCommandBase, IAsyncVoidCommandHandler<SaveGameSettings>
    {
        public SaveGameSettingsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(SaveGameSettings request) {
            await GameContext.Load(request.Id).ConfigureAwait(false);
            var game = await GameContext.Games.FindOrThrowFromRequestAsync(request).ConfigureAwait(false);
            // TODO: Specific game settings types..
            //game.UpdateSettings((Mini.Core.Games.GameSettings)Mapper.Map(request.Settings, game.Settings, request.Settings.GetType(), game.Settings.GetType()));
            Mapper.DynamicMap(request.Settings, game.Settings, request.Settings.GetType(), game.Settings.GetType());
            game.Settings.StartupParameters.StartupLine = ((dynamic) request.Settings).startupLine;
            game.UpdateSettings(game.Settings);
            await GameContext.SaveChanges().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}