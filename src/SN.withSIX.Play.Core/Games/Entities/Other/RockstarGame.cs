// <copyright company="SIX Networks GmbH" file="RockstarGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public abstract class RockstarGame : Game
    {
        protected RockstarGame(Guid id, GameSettings settings) : base(id, settings) {}
    }
}