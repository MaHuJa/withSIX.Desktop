// <copyright company="SIX Networks GmbH" file="ServersQueryExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;

namespace SN.withSIX.Play.Core.Games.Services
{
    public static class ServersQueryExtensions
    {
        public static async Task<IEnumerable<ServerQueryResult>> ServersQuery(
            this IEnumerable<ServersQuery> serverQueryInfos,
            IGameServerQueryHandler service) {
            var result = Enumerable.Empty<ServerQueryResult>();
            foreach (var q in serverQueryInfos)
                result = Enumerable.Concat(result, await service.Query((dynamic) q).ConfigureAwait(false));

            return result;
        }
    }
}