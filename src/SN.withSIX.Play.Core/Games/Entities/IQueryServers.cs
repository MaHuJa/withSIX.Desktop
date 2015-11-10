// <copyright company="SIX Networks GmbH" file="IQueryServers.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface IQueryServers
    {
        Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler);
        Task QueryServer(ServerQueryState state);
    }
}