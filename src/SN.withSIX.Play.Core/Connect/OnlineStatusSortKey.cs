// <copyright company="SIX Networks GmbH" file="OnlineStatusSortKey.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Connect
{
    [DoNotObfuscateType]
    public enum OnlineStatusSortKey
    {
        InviteRequest = -1,
        Playing = 0,
        Online = 1,
        Busy = 2,
        Away = 9,
        Group = 99,
        Offline = 100,
        MyInviteRequest = 101
    }
}