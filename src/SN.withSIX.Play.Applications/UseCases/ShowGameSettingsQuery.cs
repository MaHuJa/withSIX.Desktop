// <copyright company="SIX Networks GmbH" file="ShowGameSettingsQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.Other;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ShowGameSettingsQuery : IRequest<GameSettingsOverlayViewModel>
    {
        public ShowGameSettingsQuery(Guid id) {
            ID = id;
        }

        public Guid ID { get; }
    }

    [StayPublic]
    public class ShowGameSettingsQueryHandler : IRequestHandler<ShowGameSettingsQuery, GameSettingsOverlayViewModel>
    {
        readonly IGameContext _context;
        readonly ExportFactory<GameSettingsOverlayViewModel> _factory;
        readonly IGameMapperConfig _mapper;

        public ShowGameSettingsQueryHandler(IGameContext context, IGameMapperConfig mapper,
            ExportFactory<GameSettingsOverlayViewModel> factory) {
            _context = context;
            _mapper = mapper;
            _factory = factory;
        }

        public GameSettingsOverlayViewModel Handle(ShowGameSettingsQuery request) {
            var vm = _factory.CreateExport();
            vm.Value.GameSettings = Map(_context.Games.Find(request.ID));
            return vm.Value;
        }

        GameSettingsDataModel Map(Game game) {
            var mapped = MapSettings((dynamic) game.Settings);
            mapped.GameId = game.Id;
            return mapped;
        }

        // TODO: Generate these methods more dynamically...
        [DoNotObfuscate]
        GameSettingsDataModel MapSettings(GameSettings settings) {
            return _mapper.Map<GameSettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        RealVirtualityGameSettingsDataModel MapSettings(RealVirtualitySettings settings) {
            return _mapper.Map<RealVirtualityGameSettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        ArmaSettingsDataModel MapSettings(ArmaSettings settings) {
            return _mapper.Map<ArmaSettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        Arma2OaSettingsDataModel MapSettings(Arma2OaSettings settings) {
            return _mapper.Map<Arma2OaSettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        Arma2CoSettingsDataModel MapSettings(Arma2CoSettings settings) {
            return _mapper.Map<Arma2CoSettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        Arma3SettingsDataModel MapSettings(Arma3Settings settings) {
            return _mapper.Map<Arma3SettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        HomeWorld2SettingsDataModel MapSettings(Homeworld2Settings settings) {
            return _mapper.Map<HomeWorld2SettingsDataModel>(settings);
        }

        [DoNotObfuscate]
        GTAVSettingsDataModel MapSettings(GTAVSettings settings) {
            return _mapper.Map<GTAVSettingsDataModel>(settings);
        }
    }
}