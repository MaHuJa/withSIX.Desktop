// <copyright company="SIX Networks GmbH" file="ActiveGameChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class ActiveGameChanged : EventArgs
    {
        public readonly Game Game;

        public ActiveGameChanged(Game game) {
            Game = game;
        }
    }
}