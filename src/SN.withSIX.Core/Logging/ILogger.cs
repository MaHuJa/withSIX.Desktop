// <copyright company="SIX Networks GmbH" file="ILogger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Logging
{
    public interface ILogger
    {
        void DebugException(string message, Exception exception);
        void WarnException(string message, Exception exception);
        void ErrorException(string message, Exception exception);
        void InfoException(string message, Exception exception);
        void TraceException(string message, Exception exception);
        void FormattedDebugException(Exception e, string message = null);
        void FormattedWarnException(Exception e, string message = null);
        void FormattedErrorException(Exception e, string message = null);
        void Info(string message);
        void Debug(string message);
        void Trace(string message);
        void Warn(string message);
        void Error(string message);
        void Vital(string message);
        void Info(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Trace(string message, params object[] args);
        void Warn(string message, params object[] args);
        void Error(string message, params object[] args);
        void Vital(string message, params object[] args);
    }
}