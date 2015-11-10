// <copyright company="SIX Networks GmbH" file="GroupContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.UseCases.Groups;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class GroupContextMenu : ContextMenuBase<GroupContactDataModel>
    {
        readonly ConnectViewModel _connect;
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;

        public GroupContextMenu(ConnectViewModel connect, IMediator mediator, IDialogManager dialogManager) {
            _connect = connect;
            _mediator = mediator;
            _dialogManager = dialogManager;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Chat_Message), DoNotObfuscate]
        public Task Chat(GroupContactDataModel entity) {
            return _connect.OpenChat(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void VisitHomepage(GroupContactDataModel entity) {
            _connect.VisitHomepage(entity);
        }

        /*
        [MenuItem(Icon = SixIcon.Info), DoNotObfuscate]
        public void VisitProfile(GroupContactDataModel entity) {
            _connect.VisitProfile(entity);
        }
        */

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public void AddMemberToGroup(GroupContactDataModel entity) {
            _connect.AddMemberToGroup(entity.Group);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task LeaveGroup(GroupContactDataModel entity) {
            return _mediator.RequestAsyncWrapped(new LeaveGroupCommand(entity.Group.Id));
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public async Task DeleteGroup(GroupContactDataModel entity) {
            if (
                (await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams("You are about to delete the group: " + entity.Group.DisplayName,
                        "Are you sure?", SixMessageBoxButton.YesNo))).IsYes())
                await _mediator.RequestAsyncWrapped(new DeleteGroupCommand(entity.Group.Id)).ConfigureAwait(false);
        }

        protected override void UpdateItemsFor(GroupContactDataModel item) {
            Items.Where(x => x.AsyncAction == DeleteGroup || x.Action == AddMemberToGroup)
                .ForEach(x => x.IsVisible = item.Group.IsMine);

            GetAsyncItem(LeaveGroup)
                .IsVisible = !item.Group.IsMine;

            GetItem(VisitHomepage)
                .IsVisible = item.Group.Homepage != null;
        }
    }
}