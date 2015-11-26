// <copyright company="SIX Networks GmbH" file="GetSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class GetSettings : IAsyncQuery<ISettingsViewModel>
    {
        public bool SelectGameTab { get; set; }
    }

    public class GetSettingsHandler : DbQueryBase, IAsyncRequestHandler<GetSettings, ISettingsViewModel>
    {
        public GetSettingsHandler(IDbContextLocator dbContextLocator)
            : base(dbContextLocator) {}

        public async Task<ISettingsViewModel> HandleAsync(GetSettings request) {
            await GameContext.LoadAll().ConfigureAwait(false);
            var vm = new SettingsViewModel(GetSettingsTabs());
            if (request.SelectGameTab)
                vm.Settings.SelectedItem = vm.Settings.Items.OfType<IGamesSettingsTabViewModel>().First();
            return vm;
        }

        IEnumerable<ISettingsTabViewModel> GetSettingsTabs() {
            // TODO: manual factor / etc through AM ?
            var vms = new ISettingsTabViewModel[] {
                new InterfaceSettingsTabViewModel {
                    Version = Consts.ProductTitle + " " + Consts.ProductVersion
                },
                new GamesSettingsTabViewModel(SettingsContext.Settings.Local.SelectedGameId,
                    GameContext.Games
                        .OrderBy(x => x.Metadata.Name)
                        .Where(x => (Consts.Features.UnreleasedGames || x.Metadata.IsPublic))
                        /*.Where(x => x.InstalledState.IsInstalled) */
                        .MapTo<List<DetectedGameItemViewModel>>())
            }.Where(x => x != null);
            foreach (var s in vms)
                Mapper.Map(SettingsContext.Settings, s, SettingsContext.Settings.GetType(), s.GetType());

            return vms;
        }
    }
}