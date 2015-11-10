// <copyright company="SIX Networks GmbH" file="InviteRequestContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class InviteRequestContextMenu : ContextMenuBase<InviteRequestContactDataModel>
    {
        readonly ConnectViewModel _connect;

        public InviteRequestContextMenu(ConnectViewModel connect) {
            _connect = connect;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void VisitProfile(InviteRequestContactDataModel entity) {
            _connect.VisitProfile(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task Accept(InviteRequestContactDataModel entity) {
            return _connect.ApproveInvite((InviteRequest) entity.Model);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task Decline(InviteRequestContactDataModel entity) {
            return _connect.DeclineInvite((InviteRequest) entity.Model);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void Hide(InviteRequestContactDataModel entity) {
            _connect.HideInvite((InviteRequest) entity.Model);
        }
    }
}