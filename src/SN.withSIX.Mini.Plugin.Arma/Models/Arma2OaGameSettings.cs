// <copyright company="SIX Networks GmbH" file="Arma2OaGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma2OaGameSettings : Arma2GameSettings
    {
        public Arma2OaGameSettings() {
            StartupParameters = new Arma2OaStartupParameters(DefaultStartupParameters);
        }
    }
}