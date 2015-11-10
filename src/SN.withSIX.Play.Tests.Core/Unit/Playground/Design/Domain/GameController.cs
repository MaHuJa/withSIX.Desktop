// <copyright company="SIX Networks GmbH" file="GameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class GameController {}

    public abstract class GameController<TGame, TGameData> : GameController
        where TGame : Game
        where TGameData : GameData
    {
        protected readonly TGame Game;

        protected GameController(TGame game) {
            Game = game;
        }

        protected abstract TGameData GetGameData();
    }
}