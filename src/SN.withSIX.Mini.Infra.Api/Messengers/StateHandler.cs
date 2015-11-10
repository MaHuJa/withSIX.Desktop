// <copyright company="SIX Networks GmbH" file="StateHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using ShortBus;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Infra.Api.Hubs;

namespace SN.withSIX.Mini.Infra.Api.Messengers
{
    public class StateHandler : INotificationHandler<GameLockChanged>,
        INotificationHandler<ContentStateChange>, INotificationHandler<LocalContentAdded>,
        INotificationHandler<ContentStatusChanged>, INotificationHandler<StatusModelChanged>,
        INotificationHandler<UninstallActionCompleted>, INotificationHandler<GameLaunched>, INotificationHandler<GameTerminated>
    {
        readonly IHubContext<IStatusClientHub> _hubContext =
            GlobalHost.ConnectionManager.GetHubContext<StatusHub, IStatusClientHub>();

        public void Handle(ContentStateChange notification) {
            _hubContext.Clients.All.ContentStateChanged(notification);
        }

        public void Handle(ContentStatusChanged notification) {
            _hubContext.Clients.All.ContentStatusChanged(new ContentStatusChangedModel(notification.Content.GameId,
                notification.Content.Id, notification.State, notification.Progress, notification.Speed));
        }

        public void Handle(GameLockChanged notification) {
            if (notification.IsLocked)
                _hubContext.Clients.All.LockedGame(notification.GameId);
            else
                _hubContext.Clients.All.UnlockedGame(notification.GameId);
        }

        public void Handle(LocalContentAdded notification) {
            _hubContext.Clients.All.ContentStateChanged(new ContentStateChange {
                GameId = notification.GameId,
                States = notification.LocalContent.GetStates()
            });
        }

        public void Handle(CollectionInstalled notification)
        {
            _hubContext.Clients.All.ContentStateChanged(new ContentStateChange
            {
                GameId = notification.GameId,
                States = new Dictionary<Guid, ContentState> {
                    { notification.ContentId, new ContentState { GameId = notification.GameId, State = ItemState.Uptodate} }
                }
            });
        }

        public void Handle(StatusModelChanged notification) {
            _hubContext.Clients.All.StatusChanged(notification.Status);
        }

        public void Handle(UninstallActionCompleted notification) {
            _hubContext.Clients.All.ContentStateChanged(new ContentStateChange {
                GameId = notification.Game.Id,
                States =
                    notification.UninstallLocalContentAction.Content.ToDictionary(x => x.Content.Id,
                        x => {
                            var lc = x.Content as LocalContent;
                            var state = lc != null ? lc.MapTo<ContentState>() : x.Content.MapTo<ContentState>();
                            state.State = ItemState.Uninstalled;
                            return state;
                        })
            });
        }

        public void Handle(GameLaunched notification) {
            _hubContext.Clients.All.LaunchedGame(notification.Game.Id);
        }

        public void Handle(GameTerminated notification) {
            _hubContext.Clients.All.TerminatedGame(notification.Game.Id);
        }
    }
}