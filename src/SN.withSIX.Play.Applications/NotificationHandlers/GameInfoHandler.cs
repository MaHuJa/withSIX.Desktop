// <copyright company="SIX Networks GmbH" file="GameInfoHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    public class GameInfoHandler : IAsyncNotificationHandler<ActiveGameChanged>,
        IAsyncNotificationHandler<SubGamesChanged>
    {
        readonly IContentManager _contentManager;

        public GameInfoHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public Task HandleAsync(ActiveGameChanged notification) {
            return _contentManager.Handle(notification);
        }

        public Task HandleAsync(SubGamesChanged notification) {
            return _contentManager.Handle(notification);
        }
    }
}