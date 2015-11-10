// <copyright company="SIX Networks GmbH" file="ModsHubMessenger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Infra.Server.Hubs
{
    public class ModsHubMessenger : BaseMessenger<ModsHub>, INotificationHandler<ModInfoChangedEvent>
    {
        public void Handle(ModInfoChangedEvent notification) {
            Context.Clients.All.ModInfoChanged(notification.ModInfo);
        }
    }
}