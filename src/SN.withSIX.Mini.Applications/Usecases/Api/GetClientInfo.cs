// <copyright company="SIX Networks GmbH" file="GetClientInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetClientInfo : IAsyncQuery<ClientInfo> {}

    public class GetClientInfoHandler : IAsyncRequestHandler<GetClientInfo, ClientInfo>
    {
        public Task<ClientInfo> HandleAsync(GetClientInfo request) {
            return
                Task.FromResult(new ClientInfo {
                    Version = Consts.ProductVersion,
                    NewVersionAvailable =
                        Consts.NewVersionAvailable == null ? null : Consts.NewVersionAvailable.ToString()
                });
        }
    }

    public class ClientInfo
    {
        public string Version { get; set; }
        public string NewVersionAvailable { get; set; }
    }
}