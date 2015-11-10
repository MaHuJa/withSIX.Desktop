// <copyright company="SIX Networks GmbH" file="ClientHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Microsoft.AspNet.SignalR;
using ShortBus;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Infra.Api.Hubs;

namespace SN.withSIX.Mini.Infra.Api.Messengers
{
    public class ClientHandler : INotificationHandler<AppStateUpdated>
    {
        readonly IHubContext<IClientClientHub> _hubContext =
            GlobalHost.ConnectionManager.GetHubContext<ClientHub, IClientClientHub>();

        public void Handle(AppStateUpdated notification) {
            _hubContext.Clients.All.AppStateUpdated(new AppState(notification.UpdateState, Consts.NewVersionAvailable));
        }
    }
}