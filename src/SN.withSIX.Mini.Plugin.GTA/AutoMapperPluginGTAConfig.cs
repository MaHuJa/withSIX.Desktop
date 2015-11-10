// <copyright company="SIX Networks GmbH" file="AutoMapperPluginGTAConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Plugin.GTA.ApiModels;
using SN.withSIX.Mini.Plugin.GTA.Models;
using SN.withSIX.Mini.Plugin.GTA.ViewModels;

namespace SN.withSIX.Mini.Plugin.GTA
{
    public class AutoMapperPluginGTAConfig
    {
        public static void Setup() {
            SetupViewModels();
            SetupApiModels();
        }

        static void SetupViewModels() {
            Cheat.MapperConfiguration.CreateMap<GTA4GameSettings, GTA4GameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<GTA4GameSettingsViewModel, GTA4GameSettings>();

            Cheat.MapperConfiguration.CreateMap<GTA5GameSettings, GTA5GameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<GTA5GameSettingsViewModel, GTA5GameSettings>();
        }

        static void SetupApiModels() {
            Cheat.MapperConfiguration.CreateMap<GTA4GameSettings, GTA4GameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<GTA4GameSettingsApiModel, GTA4GameSettings>();

            Cheat.MapperConfiguration.CreateMap<GTA5GameSettings, GTA5GameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<GTA5GameSettingsApiModel, GTA5GameSettings>();
        }
    }
}