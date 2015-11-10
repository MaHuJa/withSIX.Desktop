// <copyright company="SIX Networks GmbH" file="Arma3GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma3GameSettings : RealVirtualityGameSettings
    {
        public Arma3GameSettings() {
            StartupParameters = new Arma3StartupParameters(DefaultStartupParameters);
        }

        [DataMember]
        public bool LaunchThroughBattlEye { get; set; } = true;
    }
}