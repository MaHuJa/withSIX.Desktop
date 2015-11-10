// <copyright company="SIX Networks GmbH" file="IHaveCustomRepo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public interface IHaveCustomRepo
    {
        IReadOnlyCollection<IMod> CustomRepoMods { get; set; }
        SixRepo CustomRepo { get; set; }
        string CustomRepoUrl { get; set; }
        string CustomRepoUuid { get; set; }
        SixRepoApp[] CustomRepoApps { get; set; }
    }
}