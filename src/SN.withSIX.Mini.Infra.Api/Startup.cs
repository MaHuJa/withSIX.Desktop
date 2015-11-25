// <copyright company="SIX Networks GmbH" file="Startup.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Core.Games;

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

        private static JsonSerializer CreateJsonSerializer() {
            return JsonSerializer.Create(new JsonSerializerSettings().SetDefaultSettings());
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
            GlobalHost.DependencyResolver.Register(typeof(IParameterResolver), () => resolver);
            app.UseCors(new MyCorsOptions());
            app.Map("/api/get-upload-folders", builder => builder.Run(InvokeGames));
            app.Map("/signalr", map => {
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
        async Task InvokeGames(IOwinContext context)
        {
            context.Response.ContentType = "application/json";
            using (var memoryStream = new MemoryStream())
            {
                context.Request.Body.CopyTo(memoryStream);
                var folders = Tools.Serialization.Json.LoadJson<List<string>>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                await
                    context.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            await Cheat.Mediator.RequestAsync(new GetFolders(folders)).ConfigureAwait(false),
                            SerializationExtension.DefaultSettings)).ConfigureAwait(false);
            }
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