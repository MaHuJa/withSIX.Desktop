// <copyright company="SIX Networks GmbH" file="ApiEventHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class ApiEventHandler : IAsyncNotificationHandler<ApiHashes>
    {
        readonly Random _random;
        readonly IContentManager _syncManager;

        public ApiEventHandler(IContentManager syncManager) {
            _syncManager = syncManager;
            _random = new Random();
        }

        public async Task HandleAsync(ApiHashes notification) {
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(3, 15))).ConfigureAwait(false);
            await _syncManager.Sync(notification, true).ConfigureAwait(false);
        }
    }
}