// <copyright company="SIX Networks GmbH" file="Tools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Core
{
    public class ToolsServices : IDomainService
    {
        public ToolsServices(IProcessManager pm, Lazy<IWCFClient> wcfClient, Lazy<IGeoIpService> geoService) {
            GeoService = geoService;
            ProcessManager = pm;
            WCFClient = wcfClient;
        }

        public Lazy<IGeoIpService> GeoService { get; }
        public IProcessManager ProcessManager { get; }
        public Lazy<IWCFClient> WCFClient { get; }
    }

    public static partial class Tools
    {
        static Lazy<IWCFClient> _wcfClient;
        public static readonly string DefaultSizeReturn = String.Empty;
        public static IProcessManager ProcessManager { get; private set; }
        public static Lazy<IGeoIpService> Geo { get; private set; }

        public static void RegisterServices(ToolsServices services) {
            ProcessManager = services.ProcessManager;
            _wcfClient = services.WCFClient;
            Geo = services.GeoService;
        }
    }
}