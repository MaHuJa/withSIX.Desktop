// <copyright company="SIX Networks GmbH" file="SaveSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Settings
{
    public class SaveSettings : IAsyncVoidCommand
    {
        public SaveSettings(IReadOnlyCollection<ISettingsTabViewModel> settings) {
            Settings = settings;
        }

        public IReadOnlyCollection<ISettingsTabViewModel> Settings { get; }
    }

    public class SaveSettingsHandler : DbCommandBase, IAsyncVoidCommandHandler<SaveSettings>
    {
        public SaveSettingsHandler(IDbContextLocator dbContextLocator)
            : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(SaveSettings request) {
            await SaveSettings(request).ConfigureAwait(false);
            await SaveGameSettings(request).ConfigureAwait(false);

            return UnitType.Default;
        }

        async Task SaveSettings(SaveSettings request) {
            foreach (var settings in request.Settings.OfType<IAppSettingsViewModel>())
                Mapper.Map(settings, SettingsContext.Settings, settings.GetType(), SettingsContext.Settings.GetType());
            await SettingsContext.SaveSettings().ConfigureAwait(false);
            await new SettingsUpdated(SettingsContext.Settings).RaiseEvent().ConfigureAwait(false);
        }

        async Task SaveGameSettings(SaveSettings request) {
            foreach (var settings in request.Settings.OfType<IGameSettingsTabViewModel>())
                await UpdateGameSettings(settings).ConfigureAwait(false);

            await GameContext.SaveChanges().ConfigureAwait(false);
        }

        async Task UpdateGameSettings(IGameSettingsTabViewModel settings) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(settings).ConfigureAwait(false);
            game.UpdateSettings(
                (GameSettings) Mapper.Map(settings, game.Settings, settings.GetType(), game.Settings.GetType()));

            // TODO: Make this visible in UI somehow..
            // TODO: Perhaps make this a background operation?
            // TODO: Only scan when the paths have actually changed..
            if (game.Id == SettingsContext.Settings.Local.SelectedGameId
                && game.InstalledState.IsInstalled)
                await game.ScanForLocalContent().ConfigureAwait(false);
        }
    }

    public class SettingsUpdated : IDomainEvent
    {
        public SettingsUpdated(Models.Settings settings) {
            Settings = settings;
        }

        public Models.Settings Settings { get; }
    }
}