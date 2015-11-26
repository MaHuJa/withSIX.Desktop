// <copyright company="SIX Networks GmbH" file="ClientHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases.Api;

namespace SN.withSIX.Mini.Infra.Api.Hubs
{
    public class ClientHub : HubBase<IClientClientHub>
    {
        public Task<ClientInfo> GetInfo() {
            return RequestAsync(new GetClientInfo());
        }

        public Task SetLogin(string apiKey) {
            return RequestAsync(new SetLogin(apiKey));
        }

        [Obsolete]
        public async Task ConfirmPremium() {
            return;
        }

        public Task Login(AccessInfo info) {
            return RequestAsync(new Applications.Usecases.Api.Login(info));
        }

        public Task PerformUpdate() {
            return RequestAsync(new PerformUpdate());
        }
    }

    public interface IClientClientHub
    {
        void AppStateUpdated(ClientInfo appState);
    }
}