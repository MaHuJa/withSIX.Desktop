// <copyright company="SIX Networks GmbH" file="ClientHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Usecases;
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

        public async Task ConfirmPremium() {
            //try {
            await RequestAsync(new ConfirmIsPremium()).ConfigureAwait(false);
            /*
            } catch (NotLoggedinException) {
                await OpenScreenCached(new GetLogin()).ConfigureAwait(false);
            }
*/
        }

        public Task PerformUpdate() {
            return RequestAsync(new PerformUpdate());
        }
    }

    public interface IClientClientHub
    {
        void AppStateUpdated(AppState appState);
    }

    public class AppState
    {
        public AppState(AppUpdateState updateState, Version newVersionAvailable) {
            UpdateState = updateState;
            NewVersionAvailable = newVersionAvailable;
        }

        public AppUpdateState UpdateState { get; }
        public Version NewVersionAvailable { get; }
    }
}