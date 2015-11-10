// <copyright company="SIX Networks GmbH" file="GeoIpService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Net;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Core.Infra.Services
{
    public class GeoIpService : IGeoIpService, IInfrastructureService
    {
        readonly LookupService _lookupService;
        readonly IResourceService _resources;

        public GeoIpService(IResourceService resources) {
            _resources = resources;
            _lookupService = new LookupService(GeoIpDb(), LookupService.GEOIP_MEMORY_CACHE);
        }

        public string GetCountryCode(IPAddress ip) {
            var country = _lookupService.getCountry(ip);
            return country == null ? null : country.getCode();
        }

        Stream GeoIpDb() {
            return _resources.GetResource("config.GeoIP.dat");
        }
    }
}