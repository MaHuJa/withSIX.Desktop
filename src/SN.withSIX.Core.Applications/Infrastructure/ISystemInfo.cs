// <copyright company="SIX Networks GmbH" file="ISystemInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface ISystemInfo : IDisposable
    {
        bool IsAVDetected { get; }
        bool IsInternetAvailable { get; }
        bool DidDetectAVRun { get; }
        IList<string> InstalledAV { get; }
        IList<string> InstalledFW { get; }
        int DirectXVersion { get; }
        bool DirectXVersionSupported(int VersionToCheck);
    }
}