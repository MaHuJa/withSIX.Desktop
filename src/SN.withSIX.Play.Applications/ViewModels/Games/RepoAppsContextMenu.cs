// <copyright company="SIX Networks GmbH" file="RepoAppsContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class RepoAppsContextMenu : ContextMenuBase, IIsEmpty
    {
        readonly IProcessManager _processManager;
        readonly IUpdateManager _updateManager;

        public RepoAppsContextMenu(IUpdateManager updateManager,
            IProcessManager processManager) {
            _updateManager = updateManager;
            _processManager = processManager;
        }

        public bool IsEmpty() {
            return !Items.Any();
        }

        public void Refresh(CustomCollection customCollection) {
            Items.Clear();

            var customRepo = customCollection == null ? null : customCollection.CustomRepo;
            if (_updateManager.IsUpdateNeeded && (customCollection == null || !customCollection.ForceModUpdate))
                Items.Add(new MenuItem("Play without updating", () => Play()));
            if (customRepo == null)
                return;

            foreach (var app in customRepo.Apps.Where(x => !x.Value.IsHidden)) {
                Items.Add(new MenuItem(app.Value.GetDisplayName(),
                    () => { _processManager.StartAndForget(new ProcessStartInfo(app.Value.Address, null)); }));
            }
        }

        // TODO: We aughta go through the actual statemachine instead :/
        Task Play() {
            return _updateManager.Play();
        }
    }
}