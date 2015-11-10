// <copyright company="SIX Networks GmbH" file="IServerQueryQueue.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public interface IServerQueryQueue
    {
        IServerQueryOverallState State { get; }
        Task SyncAsync(Server[] objects);
        Task SyncAsync(Server[] objects, CancellationToken token);
    }

    public interface IServerQueryOverallState : IProgressState
    {
        int Canceled { get; set; }
        void IncrementProcessed();
        void IncrementCancelled();
    }
}