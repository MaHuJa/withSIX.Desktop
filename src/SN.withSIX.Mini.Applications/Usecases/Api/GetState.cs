// <copyright company="SIX Networks GmbH" file="GetState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetState : IAsyncQuery<ClientContentInfo>, IHaveId<Guid>
    {
        public GetState(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetStateHandler : ApiDbQueryBase, IAsyncRequestHandler<GetState, ClientContentInfo>
    {
        readonly GameLockMonitor _monitor;
        readonly IStateHandler _stateHandler;

        public GetStateHandler(IDbContextLocator dbContextLocator, GameLockMonitor monitor, IStateHandler stateHandler)
            : base(dbContextLocator) {
            _monitor = monitor;
            _stateHandler = stateHandler;
        }

        public async Task<ClientContentInfo> HandleAsync(GetState request) {
            var game = await GameContext.FindGameFromRequestOrThrowAsync(request).ConfigureAwait(false);
            var gameLock = await _monitor.GetObservable(request.Id).FirstAsync();
            var gameStateHandler = _stateHandler.Games[game.Id];
            return new ClientContentInfo {
                GameLock = gameLock,
                Content = gameStateHandler.State.ToDictionary(x => x.Key, x => x.Value),
                IsRunning = gameStateHandler.IsRunning,
                Status = _stateHandler.Status
            };
        }
    }

    public class ClientContentInfo
    {
        public Dictionary<Guid, ContentState> Content { get; set; }
        public bool GameLock { get; set; }
        public StatusModel Status { get; set; }
        public bool IsRunning { get; set; }
    }
}