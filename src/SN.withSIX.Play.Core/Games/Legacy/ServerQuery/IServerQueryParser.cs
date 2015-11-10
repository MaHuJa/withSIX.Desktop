// <copyright company="SIX Networks GmbH" file="IServerQueryParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public interface IServerQueryParser
    {
        ServerQueryResult ParsePackets(ServerQueryState state);
    }
}