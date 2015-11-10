// <copyright company="SIX Networks GmbH" file="AutoMapperPluginHomeworldConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Plugin.Homeworld.ApiModels;
using SN.withSIX.Mini.Plugin.Homeworld.Models;
using SN.withSIX.Mini.Plugin.Homeworld.ViewModels;

namespace SN.withSIX.Mini.Plugin.Homeworld
{
    public class AutoMapperPluginHomeworldConfig
    {
        public static void Setup() {

            SetupViewModels(); SetupApiModels();
        }

        static void SetupViewModels() {
            Cheat.MapperConfiguration.CreateMap<Homeworld2GameSettings, Homeworld2GameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<Homeworld2GameSettingsViewModel, Homeworld2GameSettings>();
        }
        static void SetupApiModels()
        {
            Cheat.MapperConfiguration.CreateMap<Homeworld2GameSettings, Homeworld2GameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<Homeworld2GameSettingsApiModel, Homeworld2GameSettings>();
        }
    }
}