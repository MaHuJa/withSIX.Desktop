// <copyright company="SIX Networks GmbH" file="GetClientInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetClientInfo : IAsyncQuery<ClientInfo> {}

    public class GetClientInfoHandler : IAsyncRequestHandler<GetClientInfo, ClientInfo>
    {
        public Task<ClientInfo> HandleAsync(GetClientInfo request) {
            return Task.FromResult(new ClientInfo());
        }
    }

    public class ClientInfo
    {
        public ClientInfo(AppUpdateState updateState = AppUpdateState.Uptodate)
        {
            UpdateState = updateState;
        }

        public AppUpdateState UpdateState { get; set; }
        public Version Version { get; } = Consts.InternalVersion;
        public Version NewVersionAvailable { get; } = Consts.NewVersionAvailable;
    }
}