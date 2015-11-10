// <copyright company="SIX Networks GmbH" file="GameLaunchedEvent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class GameLaunchedEvent : TimeStampedEvent
    {
        public readonly RunningGame RunningGame;
        public readonly Server Server;

        public GameLaunchedEvent(RunningGame runningGame, Server server = null) {
            Contract.Requires<ArgumentNullException>(runningGame != null);
            RunningGame = runningGame;
            Server = server;
        }
    }
}