// <copyright company="SIX Networks GmbH" file="ContentHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Microsoft.AspNet.SignalR;
using ShortBus;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Infra.Api.Hubs;

namespace SN.withSIX.Mini.Infra.Api.Messengers
{
    public class ContentHandler : INotificationHandler<ContentFavorited>, INotificationHandler<ContentUnFavorited>,
        INotificationHandler<ContentUsed>, INotificationHandler<LocalContentAdded>
    {
        readonly IHubContext<IContentClientHub> _hubContext =
            GlobalHost.ConnectionManager.GetHubContext<ContentHub, IContentClientHub>();

        public void Handle(ContentFavorited notification) {
            _hubContext.Clients.All.ContentFavorited(notification.Content.GameId,
                notification.Content.MapTo<FavoriteContentModel>());
        }

        public void Handle(ContentUnFavorited notification) {
            _hubContext.Clients.All.ContentUnfavorited(notification.Content.GameId, notification.Content.Id);
        }

        public void Handle(ContentUsed notification) {
            //_hubContext.Clients.All.RecentItemAdded(notification.Content.GameId,
            //  notification.Content.MapTo<RecentContentModel>());
            _hubContext.Clients.All.RecentItemUsed(notification.Content.GameId, notification.Content.Id,
                notification.Content.RecentInfo.LastUsed);
        }

        public void Handle(LocalContentAdded notification) {
            // TODO: Also have List<> based S-IR event instead?
            foreach (var c in notification.LocalContent) {
                _hubContext.Clients.All.ContentInstalled(notification.GameId,
                    c.MapTo<InstalledContentModel>());
            }
        }

/*        public void Handle(CollectionInstalled notification)
        {
            _hubContext.Clients.All.ContentInstalled(notification.GameId, new InstalledContentModel { ContentId = notification.ContentId });
        }*/
    }

    public class ContentStatusChangedModel
    {
        public ContentStatusChangedModel(Guid gameId, Guid contentId, ItemState state, double progress = 0,
            double speed = 0) {
            if (progress.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(progress), "NaN");
            if (speed.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(speed), "NaN");
            if (progress < 0)
                throw new ArgumentOutOfRangeException(nameof(progress), "Below 0");
            if (speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Below 0");
            GameId = gameId;
            ContentId = contentId;
            State = state;
            Progress = progress;
            Speed = speed;
        }

        public Guid GameId { get; }
        public Guid ContentId { get; }
        public ItemState State { get; }
        public double Progress { get; }
        public double Speed { get; }
    }
}