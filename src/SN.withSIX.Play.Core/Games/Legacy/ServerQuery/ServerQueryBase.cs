// <copyright company="SIX Networks GmbH" file="ServerQueryBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Logging;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public abstract class ServerQueryBase : IEnableLogging
    {
        const int DefaultTimeoutSeconds = 5;
        protected const int DefaultSendTimeout = DefaultTimeoutSeconds*1000;
        protected const int DefaultReceiveTimeout = DefaultTimeoutSeconds*1000;
    }
}