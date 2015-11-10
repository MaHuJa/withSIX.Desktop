// <copyright company="SIX Networks GmbH" file="TakeOnHelicoptersStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public class TakeOnHelicoptersStartupParameters : RealVirtualityStartupParameters
    {
        public TakeOnHelicoptersStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}