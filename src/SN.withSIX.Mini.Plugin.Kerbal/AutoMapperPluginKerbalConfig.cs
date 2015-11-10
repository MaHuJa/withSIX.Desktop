// <copyright company="SIX Networks GmbH" file="AutoMapperPluginKerbalConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Plugin.Kerbal.ApiModels;
using SN.withSIX.Mini.Plugin.Kerbal.Models;
using SN.withSIX.Mini.Plugin.Kerbal.ViewModels;

namespace SN.withSIX.Mini.Plugin.Kerbal
{
    public class AutoMapperPluginKerbalConfig
    {
        public static void Setup() {
            SetupViewModels();
            SetupApiModels();
        }

        static void SetupViewModels() {
            Cheat.MapperConfiguration.CreateMap<KerbalSPGameSettings, KerbalSPGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<KerbalSPGameSettingsViewModel, KerbalSPGameSettings>();
        }

        static void SetupApiModels() {
            Cheat.MapperConfiguration.CreateMap<KerbalSPGameSettings, KerbalSPGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<KerbalSPGameSettingsApiModel, KerbalSPGameSettings>();
        }
    }
}