// <copyright company="SIX Networks GmbH" file="QueueHubMessenger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Infra.Api.Hubs;

namespace SN.withSIX.Mini.Infra.Api.Messengers
{
    public class QueueHubMessenger : IInfrastructureService, IQueueHubMessenger
    {
        readonly IHubContext<IQueueClientHub> _hubContext =
            GlobalHost.ConnectionManager.GetHubContext<QueueHub, IQueueClientHub>();

        public Task AddToQueue(QueueItem item) {
            return _hubContext.Clients.All.Added(item);
        }

        public Task RemoveFromQueue(Guid id) {
            return _hubContext.Clients.All.Removed(id);
        }

        public Task Update(QueueItem item) {
            return _hubContext.Clients.All.Updated(new QueueUpdate {Id = item.Id, Item = item});
        }
    }
}