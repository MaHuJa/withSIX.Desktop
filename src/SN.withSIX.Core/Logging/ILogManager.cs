// <copyright company="SIX Networks GmbH" file="ILogManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Logging
{
    public interface ILogManager
    {
        ILogger GetLogger(string type);
        ILogger GetCurrentClassLogger();
        ILogger GetCurrentClassLogger(Type loggerType);
        ILogger GetCurrentClassLoggerOrMerged();
        ILogger GetCurrentClassLoggerOrMerged(Type loggerType);
        ILogger GetLoggerOrMerged(Type type);
    }
}