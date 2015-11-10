// <copyright company="SIX Networks GmbH" file="GameBarMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.DataModels.Games;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class GameBarMenu : ContextMenuBase<GameDataModel>
    {
        readonly GamesViewModel _library;

        public GameBarMenu(GamesViewModel library) {
            _library = library;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info)]
        public void ShowInfo(GameDataModel game) {
            _library.ShowInfo(game);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Settings)]
        public void Settings(GameDataModel game) {
            _library.ShowSettings(game);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Support)]
        public void Support(GameDataModel game) {
            _library.ShowSupport(game);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Card_Purchase)]
        public void Purchase(GameDataModel game) {
            _library.PurchaseGame(game);
        }

        protected override void UpdateItemsFor(GameDataModel item) {
            base.UpdateItemsFor(item);
            GetItem(Purchase)
                .IsVisible = !item.IsInstalled;
        }
    }
}