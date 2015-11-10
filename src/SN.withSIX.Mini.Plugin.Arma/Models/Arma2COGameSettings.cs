// <copyright company="SIX Networks GmbH" file="Arma2COGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using NDepend.Path;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma2COGameSettings : Arma2OaGameSettings
    {
        public Arma2COGameSettings() {
            StartupParameters = new Arma2COStartupParameters(DefaultStartupParameters);
        }

        public IAbsoluteDirectoryPath Arma2GameDirectory { get; set; }
        [DataMember]
        protected string Arma2GameDirectoryInternal { get; set; }
        // WOrkaround frigging json .net 7.0 converter issue/!?!
        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            Arma2GameDirectory = Arma2GameDirectoryInternal?.ToAbsoluteDirectoryPath();
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            Arma2GameDirectoryInternal = Arma2GameDirectory?.ToString();
        }
    }
}