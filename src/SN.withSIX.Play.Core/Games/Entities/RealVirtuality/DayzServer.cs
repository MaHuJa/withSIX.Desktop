// <copyright company="SIX Networks GmbH" file="DayzServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Api.Models;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class DayzServer : RealVirtualityServer<DayZGame>
    {
        public DayzServer(DayZGame game, ServerAddress address) : base(game, address) {}
    }
}