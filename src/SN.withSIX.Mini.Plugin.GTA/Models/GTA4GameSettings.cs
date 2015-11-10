// <copyright company="SIX Networks GmbH" file="GTA4GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    [DataContract]
    public class GTA4GameSettings : GameSettings
    {
        public GTA4GameSettings() {
            StartupParameters = new GTA4StartupParameters();
        }
    }
}