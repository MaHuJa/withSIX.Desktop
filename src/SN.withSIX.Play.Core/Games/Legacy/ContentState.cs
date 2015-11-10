// <copyright company="SIX Networks GmbH" file="ContentState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DoNotObfuscateType]
    public enum ContentState
    {
        NotInstalled,
        Unverified,
        UpdateAvailable,
        Uptodate,
        Incompatible,
        Local
    }
}