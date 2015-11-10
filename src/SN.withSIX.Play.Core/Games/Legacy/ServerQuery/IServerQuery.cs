// <copyright company="SIX Networks GmbH" file="IServerQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public interface IServerQuery
    {
        Task UpdateAsync(ServerQueryState state);
    }
}