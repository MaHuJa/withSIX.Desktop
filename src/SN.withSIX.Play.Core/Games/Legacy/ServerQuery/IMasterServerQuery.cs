// <copyright company="SIX Networks GmbH" file="IMasterServerQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public interface IMasterServerQuery
    {
        Task<IEnumerable<ServerQueryResult>> GetParsedServers(bool forceLocal = false,
            int limit = 0);
    }
}