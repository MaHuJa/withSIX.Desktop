// <copyright company="SIX Networks GmbH" file="Arma1GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma1GameSettings : RealVirtualityGameSettings
    {
        public Arma1GameSettings() {
            StartupParameters = new Arma1StartupParameters(DefaultStartupParameters);
        }
    }
}