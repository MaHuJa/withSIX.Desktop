// <copyright company="SIX Networks GmbH" file="Arma2FreeServer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma2FreeServer : RealVirtualityServer<Arma2FreeGame>
    {
        public Arma2FreeServer(Arma2FreeGame game, ServerAddress address) : base(game, address) {}
    }
}