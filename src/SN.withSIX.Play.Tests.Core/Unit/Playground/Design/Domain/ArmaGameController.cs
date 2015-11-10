// <copyright company="SIX Networks GmbH" file="ArmaGameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;
using Mod = SN.withSIX.Play.Core.Games.Legacy.Mods.Mod;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class ArmaGameController : RealVirtualityGameController<ArmaGame, ArmaGameData>,
        ISupportProcessableContent<ArmaGameData>, ISupportLaunchableContent<RealVirtualityLaunchGlobalState>
    {
        public ArmaGameController(ArmaGame game) : base(game) {}

        public Task Launch(IEnumerable<ILaunchableContent<RealVirtualityLaunchGlobalState>> items,
            IMediator mediator) {
            throw new NotImplementedException();
        }

        //public ContentContainer<Mod2<ArmaGameData>>[] ModContainers { get; set; }
        //public ContentContainer<Mission2<ArmaGameData>>[] MissionContainers { get; set; }

        public Task Install(IProcessableContent<ArmaGameData> item, IMediator mediator) {
            return item.Install(GetGameData(), mediator);
        }

        public Task Uninstall(IProcessableContent<ArmaGameData> item, IMediator mediator) {
            return item.Uninstall(GetGameData(), mediator);
        }

        public Task Verify(IProcessableContent<ArmaGameData> item, IMediator mediator) {
            return item.Verify(GetGameData(), mediator);
        }

        public List<RealVirtualityMod<ArmaGameData>> GetMods(IEnumerable<Mod> mods) {
            // TODO: AutoMapper?? but then preferably handled from an infrastructure service somehow?
            return mods.Where(x => Game.SupportsContent(x))
                .Select(
                    x =>
                        new RealVirtualityMod<ArmaGameData>(x.Id, new PackageItem((string) null, null, null),
                            new ModMetaData()))
                .ToList();
        }

        protected override ArmaGameData GetGameData() {
            var installedState = Game.InstalledState;
            return new ArmaGameData {
                Directory = installedState.Directory,
                ModPaths = Game.ModPaths,
                MissionPaths = Game.MissionPaths
            };
        }
    }
}