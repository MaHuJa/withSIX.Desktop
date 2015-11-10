// <copyright company="SIX Networks GmbH" file="ISupportSteamSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface ISupportSteamSettings
    {
        bool LaunchUsingSteam { get; set; }
        bool ResetGameKeyEachLaunch { get; set; }
    }
}