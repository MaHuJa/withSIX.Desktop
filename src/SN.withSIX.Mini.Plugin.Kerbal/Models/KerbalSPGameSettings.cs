// <copyright company="SIX Networks GmbH" file="KerbalSPGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Kerbal.Models
{
    [DataContract]
    public class KerbalSPGameSettings : GameSettings
    {
        public KerbalSPGameSettings() {
            StartupParameters = new KerbalSPStartupParameters();
        }
    }
}