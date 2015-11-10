// <copyright company="SIX Networks GmbH" file="ArmaGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public abstract class ArmaGame : RealVirtualityGame
    {
        protected ArmaGame(Guid id, RealVirtualityGameSettings settings) : base(id, settings) {}

        public IEnumerable<string> GetProfiles() {
            throw new NotImplementedException();
        }
    }
}