// <copyright company="SIX Networks GmbH" file="InitializeResult.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public enum InitializeResult
    {
        Success = 0,
        FailureUnknown = 100,
        SteamNotStarted = 101,
        BadAppId = 102,
        MismatchedAppId = 103
    }
}