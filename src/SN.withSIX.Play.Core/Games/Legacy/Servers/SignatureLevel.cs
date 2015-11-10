// <copyright company="SIX Networks GmbH" file="SignatureLevel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy.Servers
{
    [DoNotObfuscateType]
    public enum SignatureLevel
    {
        None,
        version1,
        version2
    }
}