// <copyright company="SIX Networks GmbH" file="Startup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Mini.Infra.Api
{
    public class Startup
    {
        public static IDisposable Start(string address, int httpsPort, int httpPort) {
            var startOptions = new StartOptions();
            if (httpPort == 0 && httpsPort == 0)
                throw new CannotOpenApiPortException("No HTTP or HTTPS ports available");
            if (httpsPort != 0)
                startOptions.Urls.Add("https://" + address + ":" + httpsPort);
            if (httpPort != 0)
                startOptions.Urls.Add("http://" + address + ":" + httpPort);
            return WebApp.Start<Startup>(startOptions);
        }

        static JsonSerializer CreateJsonSerializer()
        {
            var settings = new JsonSerializerSettings().SetDefaultSettings();
            var serializer = JsonSerializer.Create(settings);
            return serializer;
        }

        public void Configuration(IAppBuilder app) {
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), CreateJsonSerializer);
            app.Map("/signalr", map => {
                // Setup the cors middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(new MyCorsOptions());

                var debug =
#if DEBUG
                    true;
#else
                    false;
#endif

                var hubConfiguration = new HubConfiguration() {
                    EnableDetailedErrors = debug
                };

                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch is already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });
        }
    }

    public class MyCorsPolicyProvider : CorsPolicyProvider
    {
        public MyCorsPolicyProvider()
        {
            PolicyResolver = context => {
                var policy = new CorsPolicy();
                ConfigurePolicy(policy);
                return Task.FromResult(policy);
            };
        }

        static void ConfigurePolicy(CorsPolicy policy)
        {
            foreach (var host in Environments.Origins)
                policy.Origins.Add(host);

            policy.AllowAnyMethod = true;
            policy.AllowAnyHeader = true;
            policy.SupportsCredentials = true;
        }
    }

    public class MyCorsOptions : CorsOptions
    {
        public MyCorsOptions()
        {
            PolicyProvider = new MyCorsPolicyProvider();
        }
    }
}