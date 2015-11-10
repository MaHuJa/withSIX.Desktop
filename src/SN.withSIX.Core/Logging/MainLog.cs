// <copyright company="SIX Networks GmbH" file="MainLog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SN.withSIX.Core.Logging
{
    public class MainLog
    {
        static readonly Lazy<ILogManager> logManager = new Lazy<ILogManager>(() => new DefaultLogManager());
        static readonly Lazy<ILogger> logger = new Lazy<ILogger>(() => LogManager.GetCurrentClassLoggerOrMerged());
        static readonly DateTime startTime = Process.GetCurrentProcess().StartTime;
        public static ILogManager LogManager
        {
            get { return logManager.Value; }
        }
        public static ILogger Logger
        {
            get { return logger.Value; }
        }

        public static Bencher Bench(string message = null, [CallerMemberName] string caller = null) {
#if DEBUG || (!MAIN_RELEASE && !BETA_RELEASE)
            return new Bencher(message, caller);
#else
            return null;
#endif
        }

        public static void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
            var ex = unhandledExceptionEventArgs.ExceptionObject as Exception;
            if (ex == null)
                Logger.Error("Catched unhandled exception in appdomain, but exception object is not of type Exception!");
            else
                Logger.FormattedErrorException(ex, "Catched unhandled exception in appdomain");
        }

        public static void BenchLog(string message) {
            var time = Tools.Generic.GetCurrentUtcDateTime - startTime;
            Logger.Debug("::BENCH:: (" + time.ToString("mm':'ss':'fff") + ") " + message);
        }
    }
}