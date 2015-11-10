// <copyright company="SIX Networks GmbH" file="Arma2COStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma2COStartupParameters : Arma2OaStartupParameters
    {
        public Arma2COStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }

    [DataContract]
    public class Arma2OaStartupParameters : Arma2StartupParameters
    {
        public Arma2OaStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }

    [DataContract]
    public class Arma2StartupParameters : ArmaStartupParameters
    {
        public Arma2StartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}