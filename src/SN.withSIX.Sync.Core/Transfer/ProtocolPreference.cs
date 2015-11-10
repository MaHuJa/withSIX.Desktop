// <copyright company="SIX Networks GmbH" file="ProtocolPreference.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Transfer
{
    [DoNotObfuscateType]
    public enum ProtocolPreference
    {
        Any = 0,
        [Description("Zsync Preferred")] PreferZsync = 1,
        [Description("Rsync Preferred")] PreferRsync,
        [Description("Zsync Only")] ZsyncOnly,
        [Description("Rsync Only")] RsyncOnly
    }
}