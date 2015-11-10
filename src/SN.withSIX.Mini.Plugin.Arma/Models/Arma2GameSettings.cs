// <copyright company="SIX Networks GmbH" file="Arma2GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma2GameSettings : RealVirtualityGameSettings
    {
        public Arma2GameSettings() {
            StartupParameters = new Arma2StartupParameters(DefaultStartupParameters);
        }
    }
}