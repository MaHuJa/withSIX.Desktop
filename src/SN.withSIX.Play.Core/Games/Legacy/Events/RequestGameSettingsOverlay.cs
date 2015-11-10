// <copyright company="SIX Networks GmbH" file="RequestGameSettingsOverlay.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class RequestGameSettingsOverlay
    {
        public RequestGameSettingsOverlay(Guid gameId) {
            GameId = gameId;
        }

        public Guid GameId { get; private set; }
    }
}