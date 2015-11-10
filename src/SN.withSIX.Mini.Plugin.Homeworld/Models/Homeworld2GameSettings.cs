// <copyright company="SIX Networks GmbH" file="Homeworld2GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Homeworld.Models
{
    [DataContract]
    public class Homeworld2GameSettings : GameSettings
    {
        public Homeworld2GameSettings() {
            StartupParameters = new Homeworld2StartupParameters();
        }
    }
}