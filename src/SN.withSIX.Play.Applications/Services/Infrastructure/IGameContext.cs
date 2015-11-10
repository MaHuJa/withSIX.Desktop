// <copyright company="SIX Networks GmbH" file="IGameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using ReactiveUI;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Applications.Services.Infrastructure
{
    public interface IGameContext : IUnitOfWork
    {
        IDbSet<Game, Guid> Games { get; }
        IDbSet<Mod, Guid> Mods { get; }
        IDbSet<Collection, Guid> Collections { get; }
        IDbSet<CustomCollection, Guid> CustomCollections { get; }
        IDbSet<SubscribedCollection, Guid> SubscribedCollections { get; }
        IDbSet<Mission, Guid> Missions { get; }
        ReactiveList<LocalModsContainer> LocalModsContainers { get; }
        ReactiveList<SixRepo> CustomRepositories { get; }
        ReactiveList<LocalMissionsContainer> LocalMissionsContainers { get; }
        void ImportCollections(ICollection<Collection> modSets);
        void ImportMissions(ICollection<Mission> missions);
        void ImportMods(ICollection<Mod> mods);
    }
}