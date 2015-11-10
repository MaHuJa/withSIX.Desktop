// <copyright company="SIX Networks GmbH" file="GameLaunchHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    public class GameLaunchHandler : IAsyncNotificationHandler<PreGameLaunchEvent>,
        IAsyncNotificationHandler<PreGameLaunchCancelleableEvent>
    {
        readonly LaunchManager _launchManager;
        readonly ExportFactory<GamesPreLaunchEventHandler> _pregameLaunchFactory;
        readonly IUpdateManager _updateManager;

        public GameLaunchHandler(IUpdateManager updateManager, LaunchManager launchManager,
            ExportFactory<GamesPreLaunchEventHandler> pregameLaunchFactory) {
            _updateManager = updateManager;
            _launchManager = launchManager;
            _pregameLaunchFactory = pregameLaunchFactory;
        }

        // TODO: Async
        public async Task HandleAsync(PreGameLaunchCancelleableEvent notification) {
            using (var handler = _pregameLaunchFactory.CreateExport())
                handler.Value.Process(notification);
        }

        public async Task HandleAsync(PreGameLaunchEvent notification) {
            _launchManager.LaunchExternalApps();
            await _updateManager.PreGameLaunch().ConfigureAwait(false);
        }
    }
}