// <copyright company="SIX Networks GmbH" file="GameMapperConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using AutoMapper;
using AutoMapper.Mappers;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.DataModels.Profiles;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.Other;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    class GameMapperConfig : AutoMapperWrapper, IGameMapperConfig
    {
        public GameMapperConfig() {
            Engine = new MappingEngine(CreateConfig());
        }

        IConfigurationProvider CreateConfig() {
            var config = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            config.SetupConverters();
            CreateProfileMap(config);
            CreateDlcMap(config);
            CreateGameMap(config);

            return config;
        }

        void CreateProfileMap(ConfigurationStore config) {
            config.CreateMap<GameSettingsProfileBase, ProfileDataModel>();
        }

        void CreateDlcMap(ConfigurationStore config) {
            config.CreateMap<DlcMetaData, DlcDataModel>();
            config.CreateMap<Dlc, DlcDataModel>()
                .AfterMap(AfterDlcDataModelMap);
        }

        void AfterDlcDataModelMap(Dlc src, DlcDataModel dest) {
            Map(src.MetaData, dest);
        }

        void CreateGameMap(ConfigurationStore config) {
            config.CreateMap<InstalledState, GameDataModel>();
            config.CreateMap<GameMetaData, GameDataModel>();
            config.CreateMap<Game, GameDataModel>()
                .AfterMap(AfterGameDataModelMap);

            CreateGameSettingsMap(config);
        }

        void CreateGameSettingsMap(ConfigurationStore config) {
            config.CreateMap<GameSettings, GameSettingsDataModel>()
                .Include<RealVirtualitySettings, RealVirtualityGameSettingsDataModel>()
                .Include<Homeworld2Settings, HomeWorld2SettingsDataModel>()
                .Include<GTAVSettings, GTAVSettingsDataModel>();

            config.CreateMap<RealVirtualitySettings, RealVirtualityGameSettingsDataModel>()
                .Include<ArmaSettings, ArmaSettingsDataModel>();

            config.CreateMap<Arma2FreeSettings, Arma2OriginalChildSettingsDataModel>();

            config.CreateMap<ArmaSettings, ArmaSettingsDataModel>()
                .Include<Arma2OaSettings, Arma2OaSettingsDataModel>();
            config.CreateMap<ArmaSettings, Arma2OriginalChildSettingsDataModel>();
            config.CreateMap<Arma2OaSettings, Arma2OaSettingsDataModel>()
                .Include<Arma3Settings, Arma3SettingsDataModel>()
                .Include<Arma2CoSettings, Arma2CoSettingsDataModel>();
            config.CreateMap<Arma2CoSettings, Arma2CoSettingsDataModel>()
                .ForMember(x => x.Arma2Free, opt => opt.Ignore())
                .ForMember(x => x.Arma2Original, opt => opt.Ignore())
                .AfterMap(AfterCoMap);

            config.CreateMap<Arma3Settings, Arma3SettingsDataModel>();
            config.CreateMap<Homeworld2Settings, HomeWorld2SettingsDataModel>();
            config.CreateMap<GTAVSettings, GTAVSettingsDataModel>();

            config.CreateMap<GameSettingsDataModel, GameSettings>()
                .Include<RealVirtualityGameSettingsDataModel, RealVirtualitySettings>()
                .Include<HomeWorld2SettingsDataModel, Homeworld2Settings>()
                .Include<GTAVSettingsDataModel, GTAVSettings>();

            config.CreateMap<RealVirtualityGameSettingsDataModel, RealVirtualitySettings>()
                .Include<ArmaSettingsDataModel, ArmaSettings>();
            config.CreateMap<ArmaSettingsDataModel, ArmaSettings>()
                .Include<Arma2OaSettingsDataModel, Arma2OaSettings>();
            config.CreateMap<Arma2OaSettingsDataModel, Arma2OaSettings>()
                .Include<Arma3SettingsDataModel, Arma3Settings>()
                .Include<Arma2CoSettingsDataModel, Arma2CoSettings>();
            config.CreateMap<Arma3SettingsDataModel, Arma3Settings>();
            config.CreateMap<Arma2CoSettingsDataModel, Arma2CoSettings>()
                .ForMember(x => x.Arma2Free, opt => opt.Ignore())
                .ForMember(x => x.Arma2Original, opt => opt.Ignore())
                .AfterMap(AfterCoRevMap);

            config.CreateMap<Arma2OriginalChildSettingsDataModel, ArmaSettings>();
            config.CreateMap<Arma2OriginalChildSettingsDataModel, Arma2FreeSettings>();

            config.CreateMap<HomeWorld2SettingsDataModel, Homeworld2Settings>();
            config.CreateMap<GTAVSettingsDataModel, GTAVSettings>();
        }

        void AfterCoRevMap(Arma2CoSettingsDataModel src, Arma2CoSettings dst) {
            Map(src.Arma2Free, dst.Arma2Free);
            Map(src.Arma2Original, dst.Arma2Original);
        }

        void AfterCoMap(Arma2CoSettings src, Arma2CoSettingsDataModel dst) {
            Map(src.Arma2Free, dst.Arma2Free);
            Map(src.Arma2Original, dst.Arma2Original);
        }

        void AfterGameDataModelMap(Game src, GameDataModel dest) {
            Map(src.MetaData, dest);
            Map(src.InstalledState, dest);

            dest.SupportsMissions = src.SupportsMissions();
            dest.SupportsMods = src.SupportsMods();
            dest.SupportsServers = src.SupportsServers();

            var iHaveDlc = src as IHaveDlc;
            if (iHaveDlc != null)
                dest.Dlcs = Map<DlcDataModel[]>(iHaveDlc.Dlcs);
        }
    }
}