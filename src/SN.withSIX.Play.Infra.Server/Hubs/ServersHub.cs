// <copyright company="SIX Networks GmbH" file="ServersHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Net;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Infra.Services;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    [DoNotObfuscateType]
    public class ServersHub : BaseHub
    {
        public ServersHub(IMediator mediator) : base(mediator) {}

        public Task<long> GetPing(string ipAddress, int queryPort, int serverPort) {
            // TODO: We will probably want to use udp ping to query port, with fallback to server port, and perhaps fallback then to icmp ping instead?
            var ip = IPAddress.Parse(ipAddress);
            var pingProvider = new PingProvider();
            return pingProvider.PingAsync(ip.ToString());
        }
    }
}