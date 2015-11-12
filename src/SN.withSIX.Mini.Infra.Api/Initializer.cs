// <copyright company="SIX Networks GmbH" file="Initializer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications;
using SN.withSIX.Mini.Applications.Core;
using SN.withSIX.Mini.Applications.Usecases.Main;
using Splat;

namespace SN.withSIX.Mini.Infra.Api
{
    public static class WebEx
    {
        public static Exception Exception;
    }

    public class Initializer : IInitializer
    {
        readonly ITokenRefresher _tokenRefresher;
        TimerWithElapsedCancellationAsync _timer;
        static IDisposable _webServer;

        public Initializer(ITokenRefresher tokenRefresher) {
            _tokenRefresher = tokenRefresher;
        }

        public Task Initialize() {
            AutoMapperInfraApiConfig.Setup();
            // TODO: ON startup or at other times too??
            _timer = new TimerWithElapsedCancellationAsync(TimeSpan.FromMinutes(30).TotalMilliseconds, OnElapsed);
            var task = OnElapsed(); // TODO: Move to somewhere while the UI is running or?
            return TaskExt.Default;
        }

        // This requires the Initializer to be a singleton, not great to have to require singleton for all?
        public Task Deinitialize() {
            _webServer?.Dispose();
            _webServer = null;
            _timer?.Dispose();
            _timer = null;
            return TaskExt.Default;
        }

        async Task<bool> OnElapsed() {
            try {
                await _tokenRefresher.RefreshTokenTask().ConfigureAwait(false);
            } catch (Exception ex) {
                MainLog.Logger.Write(ex.Format(), LogLevel.Warn);
            }
            return true;
        }

        public static void TryLaunchWebserver() {
            try
            {
                SetupWebServer();
            }
            catch (CannotOpenApiPortException ex)
            {
                MainLog.Logger.Write(
                    "We were unable to open the required port for the website to communicate with the client: \n" +
                    ex.Format(), LogLevel.Error);
                WebEx.Exception = ex;
            }
        }

        static void SetupWebServer() {
            // TODO: Try multiple ports?
            const int maxTries = 10;
            const int timeOut = 1500;
            var tries = 0;

            var sslPort = Consts.SrvPort;
            var httpPort = Consts.SrvHttpPort;

            using (var pm = new ProcessManager()) {
                var si = new ServerInfo(pm);
                if (!si.SslRegistered())
                    sslPort = 0;
                if (!si.isHttpRegistered)
                    httpPort = 0;
            }

            retry:
            try {
                _webServer = Startup.Start(Consts.SrvAddress, sslPort, httpPort);
            } catch (TargetInvocationException ex) {
                var unwrapped = ex.UnwrapExceptionIfNeeded();
                if (!(unwrapped is HttpListenerException))
                    throw;

                if (tries++ >= maxTries)
                    throw GetCustomException(unwrapped, Consts.SrvPort);
                MainLog.Logger.Write(unwrapped.Format(), LogLevel.Warn);
                Thread.Sleep(timeOut);
                goto retry;
            } catch (HttpListenerException ex) {
                if (tries++ >= maxTries)
                    throw GetCustomException(ex, Consts.SrvPort);
                MainLog.Logger.Write(ex.Format(), LogLevel.Warn);
                Thread.Sleep(timeOut);
                goto retry;
            }
        }

        static Exception GetCustomException(Exception unwrapped, int port) {
            return new CannotOpenApiPortException("The port: " + port + " is already in use?\n" + unwrapped.Message,
                unwrapped);
        }
    }


    public class ServerInfo
    {
        public static string value = Consts.SrvAddress + ":" + Consts.SrvPort;
        public static string valueHttp = Consts.SrvAddress + ":" + Consts.SrvHttpPort;

        public ServerInfo(IProcessManagerSync pm) {
            isHttpRegistered = IsHttpRegistered(pm, "http://" + valueHttp);
            isHttpsRegistered = IsHttpRegistered(pm, "https://" + value);
            isCertRegistered = IsCertRegistered(pm, value);
            MainLog.Logger.Write(
                "HttpRegistered: " + isHttpRegistered + ", HttpsRegistered: " + isHttpsRegistered + ", CertRegistered: " +
                isCertRegistered, LogLevel.Info);
        }

        public bool isCertRegistered { get; }
        public bool isHttpsRegistered { get; }
        public bool isHttpRegistered { get; }

        public bool AllRegistered() {
            return isHttpRegistered && SslRegistered();
        }

        public bool SslRegistered() {
            return isHttpsRegistered && isCertRegistered;
        }

        static bool IsHttpRegistered(IProcessManagerSync pm, string value) {
            var output = pm.LaunchAndGrabToolCmd(new ProcessStartInfo("netsh", "http show urlacl"), "netsh");
            return output.StandardOutput.Contains(value)
                   || output.StandardError.Contains(value);
        }

        static bool IsCertRegistered(IProcessManagerSync pm, string value) {
            var output = pm.LaunchAndGrabToolCmd(new ProcessStartInfo("netsh", "http show sslcert"), "netsh");
            return output.StandardOutput.Contains(value)
                   || output.StandardError.Contains(value);
        }
    }

    public class CannotOpenApiPortException : Exception
    {
        public CannotOpenApiPortException(string message) : base(message) {}
        public CannotOpenApiPortException(string message, Exception ex) : base(message, ex) {}
    }
}