// <copyright company="SIX Networks GmbH" file="TakeOnMarsStartupParams.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    // TODO
    [DataContract]
    public class TakeOnMarsStartupParams : BasicGameStartupParameters
    {
        public TakeOnMarsStartupParams(params string[] defaultParameters) : base(defaultParameters) {}
    }
}