// <copyright company="SIX Networks GmbH" file="Startup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
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
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings().SetDefaultSettings();

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

        private static JsonSerializer CreateJsonSerializer() {
            return JsonSerializer.Create(jsonSerializerSettings);
        }

        class Resolver : DefaultParameterResolver
        {
            private readonly JsonSerializer _serializer;

            public Resolver(JsonSerializer serializer)
            {
                _serializer = serializer;
            }

            private FieldInfo _valueField;
            public override object ResolveParameter(ParameterDescriptor descriptor, Microsoft.AspNet.SignalR.Json.IJsonValue value)
            {
                if (value.GetType() == descriptor.ParameterType)
                {
                    return value;
                }

                if (_valueField == null)
                    _valueField = value.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);

                var json = (string)_valueField.GetValue(value);
                using (var reader = new StringReader(json))
                    return _serializer.Deserialize(reader, descriptor.ParameterType);
            }
        }

        public void Configuration(IAppBuilder app) {
            var serializer = CreateJsonSerializer();
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
            var resolver = new Resolver(serializer);
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

                var hubConfiguration = new HubConfiguration {
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
        public MyCorsPolicyProvider() {
            PolicyResolver = context => {
                var policy = new CorsPolicy();
                ConfigurePolicy(policy);
                return Task.FromResult(policy);
            };
        }

        static void ConfigurePolicy(CorsPolicy policy) {
            foreach (var host in Environments.Origins)
                policy.Origins.Add(host);

            policy.AllowAnyMethod = true;
            policy.AllowAnyHeader = true;
            policy.SupportsCredentials = true;
        }
    }

    public class MyCorsOptions : CorsOptions
    {
        public MyCorsOptions() {
            PolicyProvider = new MyCorsPolicyProvider();
        }
    }
}