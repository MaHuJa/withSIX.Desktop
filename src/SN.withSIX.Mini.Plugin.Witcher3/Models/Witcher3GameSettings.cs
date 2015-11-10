// <copyright company="SIX Networks GmbH" file="Witcher3GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Witcher3.Models
{
    [DataContract]
    public class Witcher3GameSettings : GameSettings
    {
        public Witcher3GameSettings() {
            StartupParameters = new Witcher3StartupParameters();
        }
    }
}