// <copyright company="SIX Networks GmbH" file="IServerList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Core.Games.Legacy.Servers
{
    public interface IServerList : IHaveReactiveItems<Server>, IDisposable
    {
        IServerQueryQueue ServerQueryQueue { get; }
        ISupportServers Game { get; }
        IFilter Filter { get; }
        bool InitialSync { get; }
        bool DownloadingServerList { get; set; }
        bool IsUpdating { get; }
        DateTime SynchronizedAt { get; set; }
        Server FindOrCreateServer(ServerAddress address, bool isFavorite = false);
        void AbortSync();
        Task GetAndUpdateAll(bool onlyWhenActive, bool forceLocal = false);
        Task UpdateServers();
    }
}