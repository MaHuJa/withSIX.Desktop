// <copyright company="SIX Networks GmbH" file="BaseMessenger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    public abstract class BaseMessenger<T> where T : IHub
    {
        protected BaseMessenger() {
            Context = GlobalHost.ConnectionManager.GetHubContext<T>();
        }

        protected IHubContext Context { get; private set; }
    }
}