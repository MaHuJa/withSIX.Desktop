// <copyright company="SIX Networks GmbH" file="AutoMapperPluginArmaConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Plugin.Arma.ApiModels;
using SN.withSIX.Mini.Plugin.Arma.Models;
using SN.withSIX.Mini.Plugin.Arma.ViewModels;

namespace SN.withSIX.Mini.Plugin.Arma
{
    public class AutoMapperPluginArmaConfig
    {
        public static void Setup() {
            SetupViewModels();
            SetupApiModels();
        }

        static void SetupViewModels() {
            Cheat.MapperConfiguration.CreateMap<Arma2COGameSettings, Arma2COGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<Arma2COGameSettingsViewModel, Arma2COGameSettings>();
            Cheat.MapperConfiguration.CreateMap<Arma3GameSettings, Arma3GameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<Arma3GameSettingsViewModel, Arma3GameSettings>();
            Cheat.MapperConfiguration.CreateMap<DayZGameSettings, DayZGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<DayZGameSettingsViewModel, DayZGameSettings>();
            Cheat.MapperConfiguration.CreateMap<IronFrontGameSettings, IronFrontGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<IronFrontGameSettingsViewModel, IronFrontGameSettings>();
            Cheat.MapperConfiguration.CreateMap<TakeOnHelicoptersGameSettings, TakeOnHelicoptersGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<TakeOnHelicoptersGameSettingsViewModel, TakeOnHelicoptersGameSettings>();
            Cheat.MapperConfiguration.CreateMap<TakeOnMarsGameSettings, TakeOnMarsGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<TakeOnMarsGameSettingsViewModel, TakeOnMarsGameSettings>();
            Cheat.MapperConfiguration.CreateMap<CarrierCommandGameSettings, CarrierCommandGameSettingsViewModel>();
            Cheat.MapperConfiguration.CreateMap<CarrierCommandGameSettingsViewModel, CarrierCommandGameSettings>();
        }

        static void SetupApiModels()
        {
            Cheat.MapperConfiguration.CreateMap<Arma2COGameSettings, Arma2COGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<Arma2COGameSettingsApiModel, Arma2COGameSettings>();
            Cheat.MapperConfiguration.CreateMap<Arma3GameSettings, Arma3GameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<Arma3GameSettingsApiModel, Arma3GameSettings>();
            Cheat.MapperConfiguration.CreateMap<DayZGameSettings, DayZGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<DayZGameSettingsApiModel, DayZGameSettings>();
            Cheat.MapperConfiguration.CreateMap<IronFrontGameSettings, IronFrontGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<IronFrontGameSettingsApiModel, IronFrontGameSettings>();
            Cheat.MapperConfiguration.CreateMap<TakeOnHelicoptersGameSettings, TakeOnHelicoptersGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<TakeOnHelicoptersGameSettingsApiModel, TakeOnHelicoptersGameSettings>();
            Cheat.MapperConfiguration.CreateMap<TakeOnMarsGameSettings, TakeOnMarsGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<TakeOnMarsGameSettingsApiModel, TakeOnMarsGameSettings>();
            Cheat.MapperConfiguration.CreateMap<CarrierCommandGameSettings, CarrierCommandGameSettingsApiModel>();
            Cheat.MapperConfiguration.CreateMap<CarrierCommandGameSettingsApiModel, CarrierCommandGameSettings>();
        }
    }
}