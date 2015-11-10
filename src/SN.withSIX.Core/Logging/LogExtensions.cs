// <copyright company="SIX Networks GmbH" file="LogExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Logging
{
    public static class LogExtensions
    {
        public static ILogger Logger(this IEnableLogging obj) {
            return MainLog.LogManager.GetLoggerOrMerged(obj.GetType());
        }
    }
}