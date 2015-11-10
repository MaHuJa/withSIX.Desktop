// <copyright company="SIX Networks GmbH" file="DayZGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class DayZGameSettings : RealVirtualityGameSettings
    {
        public DayZGameSettings() {
            StartupParameters = new DayZStartupParameters(DefaultStartupParameters);
        }
    }
}