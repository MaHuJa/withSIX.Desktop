// <copyright company="SIX Networks GmbH" file="SettingsChangedHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Mini.Applications.Usecases.Settings;

namespace SN.withSIX.Mini.Applications.NotificationHandlers
{
    public class SettingsChangedHandler : IAsyncNotificationHandler<SettingsUpdated>
    {
        readonly StartWithWindowsHandler _startWithWindowsHandler = new StartWithWindowsHandler();

        public async Task HandleAsync(SettingsUpdated notification) {
            _startWithWindowsHandler.HandleStartWithWindows(notification.Settings.Local.StartWithWindows);
        }
    }
}