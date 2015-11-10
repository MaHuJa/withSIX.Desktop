// <copyright company="SIX Networks GmbH" file="MainLog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.CompilerServices;
using SN.withSIX.Core.Logging;
using DefaultLogManager = Splat.DefaultLogManager;
using ILogger = Splat.ILogger;
using ILogManager = Splat.ILogManager;

namespace SN.withSIX.Mini.Applications.Core
{
    public class MainLog
    {
        static readonly Lazy<ILogManager> logManager = new Lazy<ILogManager>(() => new DefaultLogManager());
        static readonly Lazy<ILogger> logger = new Lazy<ILogger>(() => LogManager.GetLogger(typeof (MainLog)));
        public static ILogManager LogManager => logManager.Value;
        public static ILogger Logger => logger.Value;

        public static Bencher Bench(string message = null, [CallerMemberName] string caller = null) {
#if DEBUG || (!MAIN_RELEASE && !BETA_RELEASE)
            return new Bencher(message, caller);
#else
            return null;
#endif
        }
    }
}