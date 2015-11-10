// <copyright company="SIX Networks GmbH" file="GTA5GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    [DataContract]
    public class GTA5GameSettings : GameSettings
    {
        public GTA5GameSettings() {
            StartupParameters = new GTA5StartupParameters();
        }
    }
}