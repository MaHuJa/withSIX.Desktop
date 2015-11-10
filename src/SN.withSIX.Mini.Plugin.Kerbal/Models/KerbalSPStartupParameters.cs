// <copyright company="SIX Networks GmbH" file="KerbalSPStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Kerbal.Models
{
    [DataContract]
    public class KerbalSPStartupParameters : UnityStartupParameters
    {
        public KerbalSPStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}