// <copyright company="SIX Networks GmbH" file="IGeoIpService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Net;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public interface IGeoIpService
    {
        string GetCountryCode(IPAddress ip);
    }
}