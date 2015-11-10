// <copyright company="SIX Networks GmbH" file="AutoMapperPluginGTAConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>


using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Plugin.Witcher3.ApiModels;
using SN.withSIX.Mini.Plugin.Witcher3.Models;
using SN.withSIX.Mini.Plugin.Witcher3.ViewModels;

namespace SN.withSIX.Mini.Plugin.Witcher3
{
    public class AutoMapperPluginWitcher3Config
    {
        public static void Setup() {
            SetupViewModels();
            SetupApiModels();
        }

        static void SetupViewModels() {
            Cheat.MapperConfiguration.CreateMap<Witcher3GameSettings, Witcher3GameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<Witcher3GameSettingsViewModel, Witcher3GameSettings>();
        }

        static void SetupApiModels() {
            Cheat.MapperConfiguration.CreateMap<Witcher3GameSettings, Witcher3GameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<Witcher3GameSettingsApiModel, Witcher3GameSettings>();
        }
    }
}