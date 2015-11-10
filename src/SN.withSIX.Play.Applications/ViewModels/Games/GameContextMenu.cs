// <copyright company="SIX Networks GmbH" file="GameContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.DataModels.Games;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class GameContextMenu : ContextMenuBase<GameDataModel>
    {
        readonly GamesViewModel _gamesViewModel;

        public GameContextMenu(GamesViewModel gamesViewModel) {
            _gamesViewModel = gamesViewModel;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info)]
        public void ShowInfo(GameDataModel game) {
            _gamesViewModel.ShowInfo(game);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void UseGame(GameDataModel entity) {
            _gamesViewModel.UseGame(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task LaunchGame(GameDataModel entity) {
            return _gamesViewModel.LaunchNow(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Settings), DoNotObfuscate]
        public void GameSettings(GameDataModel entity) {
            _gamesViewModel.ShowSettings(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Folder), DoNotObfuscate]
        public void OpenGameFolder(GameDataModel entity) {
            _gamesViewModel.OpenGameFolder(entity);
        }

        protected override void UpdateItemsFor(GameDataModel item) {
            base.UpdateItemsFor(item);

            GetItem(OpenGameFolder)
                .IsEnabled = item.IsInstalled;
        }
    }
}