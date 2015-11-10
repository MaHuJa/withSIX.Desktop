// <copyright company="SIX Networks GmbH" file="ModBarMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class ModBarMenu : ModMenuBase<IMod>
    {
        public ModBarMenu(ModLibraryViewModel library) : base(library) {
            this.WhenAnyValue(x => x.CurrentItem.Controller.IsInstalled, x => x.CurrentItem.State)
                .Select(x => new {CurrentItem, x})
                .Subscribe(info => {
                    Items.Where(x => x.AsyncAction == Uninstall) //  || x.AsyncAction == LaunchMod
                        .ForEach(x => x.IsVisible = info.x.Item1);

                    var installAction = GetAsyncItem(Diagnose);
                    installAction.Name = ModController.ConvertState(info.x.Item2);
                    installAction.IsVisible = installAction.Name != null && !(info.CurrentItem is LocalMod);
                });
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info)]
        public void ShowInfo(IMod mod) {
            Library.ShowInfo(mod);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Settings)]
        public void Settings(IMod mod) {
            Library.ShowSettings(mod);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Tools)]
        public Task Diagnose(IMod mod) {
            return Library.Diagnose(mod);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Folder_Remove)]
        public Task Uninstall(IMod mod) {
            return Library.Uninstall(mod);
        }

        /*        [MenuItem]
        public void AddTo() { }*/

        protected override void UpdateItemsFor(IMod item) {
            base.UpdateItemsFor(item);

            var settingsAction = GetItem(Settings);
            settingsAction.IsVisible = item.UserConfig != null;
        }
    }
}