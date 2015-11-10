// <copyright company="SIX Networks GmbH" file="DayZStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class DayZStartupParameters : ArmaStartupParameters
    {
        public DayZStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}