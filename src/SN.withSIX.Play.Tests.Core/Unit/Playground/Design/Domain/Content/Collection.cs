// <copyright company="SIX Networks GmbH" file="Collection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public abstract class Collection : Content
    {
        protected Collection(Guid id) : base(id) {}

        public void EnableMod(Mod mod) {
            throw new NotImplementedException();
        }

        public void DisableMod(Mod mod) {
            throw new NotImplementedException();
        }
    }


    // TODO: Mods should be able to have different versions within different collections
    // Also a mod can be enabled/disabled within a collection (IsEnabled)
    // A mod might be Required (IsRequired)
    public class Collection<TGameData, TLaunchSomething> : Collection, IProcessableContent<TGameData>,
        ILaunchableContent<TLaunchSomething>
        where TGameData : IModdingGameData
        //where TMod : Mod, IProcessableContent<TGameData>
    {
        public Collection(Guid id) : base(id) {}

        public CollectionMod<TGameData, Mod<TGameData, TLaunchSomething>>[] Mods { get; set; }
        public IEnumerable<ILaunchableContent<TLaunchSomething>> Dependencies { get; private set; }

        public void MakeLaunchState(TLaunchSomething sharedLaunchState, IMediator domainMediator) {
            throw new NotImplementedException();
        }

        public async Task Install(TGameData gameData, IMediator mediator) {
            foreach (var m in GetMods())
                await m.Install(gameData, mediator).ConfigureAwait(false);
        }

        public async Task Uninstall(TGameData gameData, IMediator mediator) {
            foreach (var m in GetMods())
                await m.Uninstall(gameData, mediator).ConfigureAwait(false);
        }

        public async Task Verify(TGameData gameData, IMediator mediator) {
            foreach (var m in GetMods())
                await m.Verify(gameData, mediator).ConfigureAwait(false);
        }

        IEnumerable<Mod<TGameData, TLaunchSomething>> GetMods() {
            return Mods.Select(x => x.Mod);
        }
    }
}