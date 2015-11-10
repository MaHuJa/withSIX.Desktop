// <copyright company="SIX Networks GmbH" file="IronFrontStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class IronFrontStartupParameters : ArmaStartupParameters
    {
        public IronFrontStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}