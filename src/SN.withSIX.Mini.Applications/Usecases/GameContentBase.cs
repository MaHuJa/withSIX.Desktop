// <copyright company="SIX Networks GmbH" file="GameContentBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public abstract class GameContentBase : IHaveGameId, INeedGameContents
    {
        protected GameContentBase(Guid gameId) {
            GameId = gameId;
        }

        public Guid GameId { get; }
    }
}