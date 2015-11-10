// <copyright company="SIX Networks GmbH" file="ISupportProfiles.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public interface ISupportProfiles
    {
        IEnumerable<string> GetProfiles();
    }
}