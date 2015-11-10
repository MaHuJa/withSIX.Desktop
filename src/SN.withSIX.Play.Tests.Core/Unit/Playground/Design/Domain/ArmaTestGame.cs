// <copyright company="SIX Networks GmbH" file="ArmaTestGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class ArmaTestGame : Arma2Game, ISupportModding, ISupportCollections, ISupportMissions
    {
        CollectionContentContainer<Collection<ArmaGameData, RealVirtualityLaunchGlobalState>>[] _collectionContainers;
        ArmaGameController _controller;
        RealVirtualityMissionContainer<ArmaGameData>[] _missionContainers;
        RealVirtualityModContainer<ArmaGameData>[] _modContainers;
        public ArmaTestGame(Guid id, GameSettingsController settingsController) : base(id, settingsController) {}

        public Task Install(CollectionId id, IMediator mediator) {
            return _controller.Install(FindCollection(id), mediator);
        }

        public Task Uninstall(CollectionId id, IMediator mediator) {
            return _controller.Install(FindCollection(id), mediator);
        }

        public Task Verify(CollectionId id, IMediator mediator) {
            return _controller.Install(FindCollection(id), mediator);
        }

        public Task Launch(CollectionId id, IMediator mediator) {
            return _controller.Launch(new[] {FindCollection(id)}, mediator);
        }

        public Task Install(MissionId id, IMediator mediator) {
            return _controller.Install(FindMission(id), mediator);
        }

        public Task Uninstall(MissionId id, IMediator mediator) {
            return _controller.Install(FindMission(id), mediator);
        }

        public Task Verify(MissionId id, IMediator mediator) {
            return _controller.Install(FindMission(id), mediator);
        }

        public Task Launch(MissionId id, IMediator mediator) {
            return _controller.Launch(new[] {FindMission(id)}, mediator);
        }

        public Task Install(ModId id, IMediator mediator) {
            return _controller.Install(FindMod(id), mediator);
        }

        public Task Uninstall(ModId id, IMediator mediator) {
            return _controller.Uninstall(FindMod(id), mediator);
        }

        public Task Verify(ModId id, IMediator mediator) {
            return _controller.Verify(FindMod(id), mediator);
        }

        public Task Launch(ModId id, IMediator mediator) {
            return _controller.Launch(new[] {FindMod(id)}, mediator);
        }

        RealVirtualityMod<ArmaGameData> FindMod(ModId id) {
            return _modContainers.SelectMany(x => x.List).First(x => x.Id == id.Id);
        }

        Collection<ArmaGameData, RealVirtualityLaunchGlobalState> FindCollection(CollectionId id) {
            return _collectionContainers.SelectMany(x => x.List).First(x => x.Id == id.Id);
        }

        Mission<ArmaGameData, RealVirtualityLaunchGlobalState> FindMission(MissionId id) {
            return _missionContainers.SelectMany(x => x.List).First(x => x.Id == id.Id);
        }
    }
}