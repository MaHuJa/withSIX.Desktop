// <copyright company="SIX Networks GmbH" file="NLogSplatLogger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NLog;
using SN.withSIX.Core.Logging;
using ILogger = Splat.ILogger;
using LogLevel = Splat.LogLevel;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    class NLogSplatLogger : ILogger
    {
        readonly Logger _logger;

        public NLogSplatLogger() {
            _logger = DefaultLogManager.Factory.GetCurrentClassLogger();
        }

        public void Write(string message, LogLevel logLevel) {
            _logger.Log(GetLogLevel(logLevel), message);
        }

        public LogLevel Level { get; set; } = LogLevel.Info;

        static NLog.LogLevel GetLogLevel(LogLevel logLevel) {
            switch (logLevel) {
            case LogLevel.Debug:
                return NLog.LogLevel.Debug;
            case LogLevel.Info:
                return NLog.LogLevel.Info;
            case LogLevel.Warn:
                return NLog.LogLevel.Warn;
            case LogLevel.Error:
                return NLog.LogLevel.Error;
            case LogLevel.Fatal:
                return NLog.LogLevel.Fatal;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}