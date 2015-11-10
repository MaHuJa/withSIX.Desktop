// <copyright company="SIX Networks GmbH" file="BaseMessenger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ShortBus;
using SN.withSIX.Play.Infra.Server.Hubs;
using SN.withSIX.Play.Infra.Server.Services.Messengers.ServerMessengers;

namespace SN.withSIX.Play.Infra.Server.Messengers
{
    public abstract class BaseMessenger<T> where T : IHub
    {
        protected BaseMessenger() {
            Context = GlobalHost.ConnectionManager.GetHubContext<T>();
        }

        protected IHubContext Context { get; }
    }

    public class InstallProgressMessenger : BaseMessenger<InstallProgressHub>,
        INotificationHandler<StatusUpdated>,
        INotificationHandler<StatusAdded>,
        INotificationHandler<StatusRemoved>
    {
        public void Handle(StatusAdded notification) {
            Context.Clients.All.StatusAdded(notification.StatusType, notification.StatusModel);
        }

        public void Handle(StatusRemoved notification) {
            Context.Clients.All.StatusRemoved(notification.StatusID);
        }

        public void Handle(StatusUpdated notification) {
            Context.Clients.All.StatusUpdated(notification);
        }
    }
}