// <copyright company="SIX Networks GmbH" file="CarrierCommandGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    // TODO
    [DataContract]
    public class CarrierCommandGameSettings : RealVirtualityGameSettings
    {
        public CarrierCommandGameSettings() {
            StartupParameters = new CarrierCommandStartupParmeters();
        }
    }
}