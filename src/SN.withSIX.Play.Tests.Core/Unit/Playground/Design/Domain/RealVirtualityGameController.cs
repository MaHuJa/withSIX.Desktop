// <copyright company="SIX Networks GmbH" file="RealVirtualityGameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public abstract class RealVirtualityGameController<TGame, TGameData> : GameController<TGame, TGameData>
        where TGame : RealVirtualityGame
        where TGameData : RealVirtualityGameData
    {
        protected RealVirtualityGameController(TGame game) : base(game) {}
    }
}