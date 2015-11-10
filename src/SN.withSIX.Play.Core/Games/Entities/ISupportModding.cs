// <copyright company="SIX Networks GmbH" file="ISupportModding.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using NDepend.Path;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface ISupportModding : ISupportContent
    {
        ContentPaths ModPaths { get; }
        GameController Controller { get; }
        bool SupportsContent(IMod mod);
        IEnumerable<LocalModsContainer> LocalModsContainers();
        IEnumerable<IAbsolutePath> GetAdditionalLaunchMods();
        void UpdateModStates(IReadOnlyCollection<IMod> mods);
    }
}