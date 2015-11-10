// <copyright company="SIX Networks GmbH" file="TakeOnMarsGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class TakeOnMarsGameSettings : RealVirtualityGameSettings
    {
        public TakeOnMarsGameSettings() {
            StartupParameters = new TakeOnMarsStartupParams();
        }
    }
}