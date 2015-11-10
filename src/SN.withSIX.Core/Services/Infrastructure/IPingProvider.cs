// <copyright company="SIX Networks GmbH" file="IPingProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public interface IPingProvider
    {
        long Ping(string hostName, int count = 3);
        Task<long> PingAsync(string hostName, int count = 3);
    }
}