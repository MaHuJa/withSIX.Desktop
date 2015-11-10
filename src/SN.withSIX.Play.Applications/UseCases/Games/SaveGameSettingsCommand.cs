// <copyright company="SIX Networks GmbH" file="SaveGameSettingsCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class SaveGameSettingsCommand : IRequest<UnitType>
    {
        public GameSettingsDataModel Settings { get; set; }
    }

    [StayPublic]
    public class SaveGameSettingsCommandHandler : IRequestHandler<SaveGameSettingsCommand, UnitType>
    {
        readonly IGameContext _context;
        readonly IGameMapperConfig _gameMapper;

        public SaveGameSettingsCommandHandler(IGameContext context, IGameMapperConfig gameMapper) {
            _context = context;
            _gameMapper = gameMapper;
        }

        public UnitType Handle(SaveGameSettingsCommand message) {
            var game = _context.Games.Find(message.Settings.GameId);

            ContentPaths modPaths = null;
            var modding = game as ISupportModding;
            if (modding != null)
                modPaths = modding.ModPaths;

            _gameMapper.Map(message.Settings, game.Settings);
            _gameMapper.Map(game.Settings, message.Settings);

            if (modding != null)
                HandleNewModPaths(modding, modding.ModPaths, modPaths);

            return UnitType.Default;
        }

        // TODO: Domain events should be raised by the domain, not by the application layer,
        // however it is very difficult in the current setup to detect if modpaths were changed due to a profile change or due to a settings change...
        void HandleNewModPaths(ISupportModding game, ContentPaths newPaths, ContentPaths oldPaths) {
            if (!newPaths.IsValid || !oldPaths.IsValid)
                return;

            if (!oldPaths.EqualPath(newPaths)) {
                if (!oldPaths.EqualRepositoryPath(newPaths))
                    Common.App.PublishDomainEvent(new ModAndSynqPathsChangedEvent(game, oldPaths, newPaths));
                else
                    Common.App.PublishDomainEvent(new ModPathChangedEvent(game, oldPaths, newPaths));
            } else if (!oldPaths.EqualRepositoryPath(newPaths))
                Common.App.PublishDomainEvent(new SynqPathChangedEvent(game, oldPaths, newPaths));
        }
    }
}