// <copyright company="SIX Networks GmbH" file="InstalledGames.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Applications.Services
{
    public class InstalledGames
    {
        public InstalledGame[] Games { get; set; }
    }

    public class InstalledGame
    {
        public Guid Id { get; set; }
        public InstalledDlc[] Dlcs { get; set; }
    }

    public class InstalledDlc
    {
        public Guid Id { get; set; }
    }

    public class InstalledGamesService : IInstalledGamesService, IApplicationService
    {
        readonly IGameContext _gameContext;

        public InstalledGamesService(IGameContext gameContext) {
            _gameContext = gameContext;
        }

        public InstalledGames GetInstalledGames() {
            // TODO: AutoMapper
            return new InstalledGames {Games = InstalledGames().Select(CreateInstalledGame).ToArray()};
        }

        IQueryable<Game> InstalledGames() {
            return _gameContext.Games.Where(x => x.InstalledState.IsInstalled);
        }

        InstalledGame CreateInstalledGame(Game game) {
            var hasDlcs = game as IHaveDlc;
            return new InstalledGame {
                Id = game.Id,
                Dlcs =
                    (hasDlcs == null
                        ? new InstalledDlc[0]
                        : InstalledDlcs(game, hasDlcs).Select(CreateInstalledDlc).ToArray())
            };
        }

        static IEnumerable<Dlc> InstalledDlcs(Game game, IHaveDlc hasDlcs) {
            return hasDlcs.Dlcs.Where(x => x.IsInstalled(game));
        }

        InstalledDlc CreateInstalledDlc(Dlc dlc) {
            return new InstalledDlc {Id = dlc.Id};
        }
    }

    public interface IInstalledGamesService
    {
        InstalledGames GetInstalledGames();
    }
}