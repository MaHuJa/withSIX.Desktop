// <copyright company="SIX Networks GmbH" file="ServerContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class ServerContextMenu : ContextMenuBase<Server>
    {
        readonly ServerLibraryViewModel _library;

        public ServerContextMenu(ServerLibraryViewModel library) {
            _library = library;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void ActivateServer(Server entity) {
            _library.ActiveItem = entity;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public void CreateCollectionWithServerMods(Server entity) {
            _library.CreateCollectionFromServer(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task JoinServer(Server entity) {
            return _library.JoinServer(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void CopyIpPortToClipboard(Server entity) {
            _library.CopyIpPortAction(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void CopyDetailsToClipboard(Server entity) {
            _library.CopyDetailsAction(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Lock), DoNotObfuscate]
        public Task ChangeServerPassword(Server entity) {
            return _library.ChangePassword(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Reload), DoNotObfuscate]
        public Task RefreshServerInfo(Server entity) {
            return _library.UpdateServer(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void ShowServerInfo(Server entity) {
            _library.ServerInfo(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void ClearServerHistory(Server entity) {
            _library.ClearHistory(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Notes), DoNotObfuscate]
        public void ShowNotes(Server entity) {
            _library.ShowNotes(entity);
        }

        protected override void UpdateItemsFor(Server item) {
            base.UpdateItemsFor(item);

            GetItem(CreateCollectionWithServerMods)
                .IsVisible = DomainEvilGlobal.SelectedGame.ActiveGame.SupportsMods();
        }
    }
}