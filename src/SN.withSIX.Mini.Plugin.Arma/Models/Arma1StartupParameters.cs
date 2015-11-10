// <copyright company="SIX Networks GmbH" file="Arma1StartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class Arma1StartupParameters : ArmaStartupParameters
    {
        public Arma1StartupParameters(params string[] defaultStartupParameters) : base(defaultStartupParameters) {}
    }
}