// <copyright company="SIX Networks GmbH" file="IronFrontGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class IronFrontGameSettings : RealVirtualityGameSettings
    {
        public IronFrontGameSettings() {
            StartupParameters = new IronFrontStartupParameters(DefaultStartupParameters);
        }
    }
}