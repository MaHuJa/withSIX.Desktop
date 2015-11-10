// <copyright company="SIX Networks GmbH" file="ClientHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    public class ClientHub : BaseHub
    {
        public ClientHub(IMediator mediator) : base(mediator) {}

        public Task<ClientInfo> GetInfo() {
            return Task.FromResult(new ClientInfo {Version = Common.App.ApplicationVersion.ToString()});
        }

        public Task<List<Guid>> GetInstalledStates() {
            throw new NotImplementedException();
        }
    }

    public class ClientInfo
    {
        public string Version { get; set; }
    }
}