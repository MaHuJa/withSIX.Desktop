// <copyright company="SIX Networks GmbH" file="SetupNlog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SN.withSIX.Core.Logging
{
    public class SetupNlog
    {
        public static void Initialize(string appName) {
            // TODO: This can cause StackOverFlow when having electron.config copied from sync.config
            var loggingConfiguration = LogManager.Configuration;
            if (loggingConfiguration == null)
                LogManager.Configuration = CreateDefaultConfig(appName);
            AppDomain.CurrentDomain.UnhandledException += MainLog.CurrentDomainOnUnhandledException;
        }

        static LoggingConfiguration CreateDefaultConfig(string appName) {
            var config = new LoggingConfiguration();
            var target = CreateDefaultFileTarget(appName);
            config.AddTarget("logfile", target);
            config.LoggingRules.Add(CreateDefaultLoggingRule(target));
            return config;
        }

        static LoggingRule CreateDefaultLoggingRule(Target target) {
            return new LoggingRule("SN.withSIX.*", LogLevel.Trace, target) {Final = true};
        }

        static FileTarget CreateDefaultFileTarget(string appName) {
            return new FileTarget {
                Name = "logfile",
                FileName =
                    "${specialfolder:folder=LocalApplicationData}/SIX Networks/" +
                    string.Format("{0}/logs/{0}.log", appName),
                ArchiveFileName =
                    "${specialfolder:folder=LocalApplicationData}/SIX Networks/" +
                    string.Format("{0}/logs/{0}", appName) + "_{#####}.log",
                ArchiveAboveSize = 1048576,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 5,
                ConcurrentWrites = true,
                CreateDirs = true
            };
        }
    }
}