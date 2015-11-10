// <copyright company="SIX Networks GmbH" file="OverallUpdateState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DoNotObfuscateType]
    public enum OverallUpdateState
    {
        NoGameFound,
        QuickPlay,
        Update,
        Play
    }
}