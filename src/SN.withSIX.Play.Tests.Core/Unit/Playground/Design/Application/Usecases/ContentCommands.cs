// <copyright company="SIX Networks GmbH" file="ContentCommands.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Presentation.Wpf.Services.Infrastructure;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain;
using Extensions = SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Extensions;
using ISupportMissions = SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.ISupportMissions;
using ISupportModding = SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.ISupportModding;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Application.Usecases
{
    class ContentCommandHandler :
        IAsyncRequestHandler<InstallModCommand, UnitType>,
        IAsyncRequestHandler<VerifyModCommand, UnitType>,
        IAsyncRequestHandler<UninstallModCommand, UnitType>,
        IAsyncRequestHandler<InstallMissionCommand, UnitType>,
        IAsyncRequestHandler<VerifyMissionCommand, UnitType>,
        IAsyncRequestHandler<UninstallMissionCommand, UnitType>,
        IAsyncRequestHandler<InstallCollectionCommand, UnitType>,
        IAsyncRequestHandler<VerifyCollectionCommand, UnitType>,
        IAsyncRequestHandler<UninstallCollectionCommand, UnitType>
    {
        readonly GameContext _context;
        readonly IMediator _mediator;

        public ContentCommandHandler(GameContext context, IMediator mediator) {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(mediator != null);
            _context = context;
            _mediator = mediator;
        }

        public Task<UnitType> HandleAsync(InstallCollectionCommand request) {
            return AsCollections(request.GameId).Install(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(InstallMissionCommand request) {
            return AsMissions(request.GameId).Install(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(InstallModCommand request) {
            return AsModding(request.GameId).Install(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(UninstallCollectionCommand request) {
            return AsCollections(request.GameId).Uninstall(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(UninstallMissionCommand request) {
            return AsMissions(request.GameId).Uninstall(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(UninstallModCommand request) {
            return AsModding(request.GameId).Uninstall(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(VerifyCollectionCommand request) {
            return AsCollections(request.GameId).Verify(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(VerifyMissionCommand request) {
            return AsMissions(request.GameId).Verify(request.ContentId, _mediator)
                .Void();
        }

        public Task<UnitType> HandleAsync(VerifyModCommand request) {
            return AsModding(request.GameId).Verify(request.ContentId, _mediator)
                .Void();
        }

        ISupportCollections AsCollections(GameId gameId) {
            return FindGame(gameId).Collections();
        }

        ISupportModding AsModding(GameId gameId) {
            return Extensions.Modding(FindGame(gameId));
        }

        ISupportMissions AsMissions(GameId gameId) {
            return Extensions.Missions(FindGame(gameId));
        }

        Game FindGame(GameId gameId) {
            return _context.Games.Find(gameId.Id);
        }
    }

    public abstract class ContentCommand<TContentId> : IAsyncRequest<UnitType> where TContentId : ContentId
    {
        protected ContentCommand(TContentId contentId, GameId gameId) {
            Contract.Requires<ArgumentNullException>(contentId != null);
            Contract.Requires<ArgumentNullException>(gameId != null);
            ContentId = contentId;
            GameId = gameId;
        }

        public TContentId ContentId { get; private set; }
        public GameId GameId { get; private set; }
    }

    public class InstallModCommand : ContentCommand<ModId>
    {
        public InstallModCommand(ModId modId, GameId gameId) : base(modId, gameId) {}
    }

    public class UninstallCollectionCommand : ContentCommand<CollectionId>, IAsyncRequest<UnitType>
    {
        public UninstallCollectionCommand(CollectionId collectionId, GameId gameId) : base(collectionId, gameId) {}
    }

    public class VerifyCollectionCommand : ContentCommand<CollectionId>, IAsyncRequest<UnitType>
    {
        public VerifyCollectionCommand(CollectionId collectionId, GameId gameId) : base(collectionId, gameId) {}
    }

    public class InstallCollectionCommand : ContentCommand<CollectionId>, IAsyncRequest<UnitType>
    {
        public InstallCollectionCommand(CollectionId collectionId, GameId gameId) : base(collectionId, gameId) {}
    }

    public class UninstallMissionCommand : ContentCommand<MissionId>, IAsyncRequest<UnitType>
    {
        public UninstallMissionCommand(MissionId missionId, GameId gameId) : base(missionId, gameId) {}
    }

    public class VerifyMissionCommand : ContentCommand<MissionId>, IAsyncRequest<UnitType>
    {
        public VerifyMissionCommand(MissionId missionId, GameId gameId) : base(missionId, gameId) {}
    }

    public class InstallMissionCommand : ContentCommand<MissionId>, IAsyncRequest<UnitType>
    {
        public InstallMissionCommand(MissionId missionId, GameId gameId) : base(missionId, gameId) {}
    }

    public class UninstallModCommand : ContentCommand<ModId>, IAsyncRequest<UnitType>
    {
        public UninstallModCommand(ModId modId, GameId gameId) : base(modId, gameId) {}
    }

    public class VerifyModCommand : ContentCommand<ModId>, IAsyncRequest<UnitType>
    {
        public VerifyModCommand(ModId modId, GameId gameId) : base(modId, gameId) {}
    }
}