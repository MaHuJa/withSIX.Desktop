// <copyright company="SIX Networks GmbH" file="UserContactContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public abstract class ContactContextMenuBase<T> : ContextMenuBase<T> where T : class
    {
        protected readonly ConnectViewModel Connect;

        protected ContactContextMenuBase(ConnectViewModel connect) {
            Connect = connect;
        }
    }

    public class UserContactContextMenuBase<T> : ContactContextMenuBase<T> where T : UserContactDataModel
    {
        public UserContactContextMenuBase(ConnectViewModel connect) : base(connect) {}

        protected void UpdateForItemBase(T item) {
            var isFriend = Connect.ContactList.IsFriend(item.Model.Id);
            GetAsyncItem(JoinServer)
                .IsVisible = isFriend;
            GetAsyncItem(AddFriend)
                .IsVisible = !isFriend && !Connect.ContactList.HasInviteRequest(item.Model.Id);
            GetAsyncItem(RemoveFriend)
                .IsVisible = isFriend;

            GetAsyncItem(MarkConversationAsRead)
                .IsVisible = isFriend && item.Friend.UnreadPrivateMessages > 0;

            /*            var mod = Connect.Mods.GetSelectedMod();
            GetAsyncItem(ShareSelectedMod)
                .IsEnabled = mod != null && !(mod is LocalMod) && !(mod is CustomRepoMod);*/

            /*
            var mission = Connect.Missions.GetSelectedMission();
            GetAsyncItem(ShareSelectedMission)
                .IsEnabled = mission != null && !mission.IsLocal;
*/

            GetAsyncItem(JoinServer)
                .IsEnabled = item.Friend != null && item.Friend.PlayingOn != null;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Chat_Message), DoNotObfuscate]
        public Task Chat(UserContactDataModel entity) {
            return Connect.OpenChat(entity);
        }

        /*

        [MenuItem(icon: SixIconFont.withSIX_icon_Share), DoNotObfuscate]
        public Task ShareSelectedMod(UserContactDataModel entity) {
            return Connect.ShareSelectedMod(entity);
        }

        [MenuItem(icon: SixIconFont.withSIX_icon_Share), DoNotObfuscate]
        public Task ShareSelectedMission(UserContactDataModel entity) {
            return Connect.ShareSelectedMission(entity);
        }
*/

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task JoinServer(UserContactDataModel entity) {
            return Connect.JoinServer(entity.Friend);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void VisitProfile(UserContactDataModel entity) {
            Connect.VisitProfile(entity);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Nav_Server), DoNotObfuscate]
        public void ViewConversationOnline(UserContactDataModel entity) {
            Connect.ViewConversationOnline(entity);
        }

        [DoNotObfuscate, MenuItem]
        public Task MarkConversationAsRead(UserContactDataModel arg) {
            return Connect.MarkAsRead(arg.Friend);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task AddFriend(UserContactDataModel entity) {
            return Connect.AddFriend(entity.Friend.Account);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task RemoveFriend(UserContactDataModel entity) {
            return Connect.RemoveContact(entity);
        }
    }


    public class UserContactContextMenu : UserContactContextMenuBase<UserContactDataModel>
    {
        public UserContactContextMenu(ConnectViewModel connect) : base(connect) {}

        protected override void UpdateItemsFor(UserContactDataModel item) {
            UpdateForItemBase(item);
        }
    }

    public class GroupMemberContextMenu : UserContactContextMenuBase<GroupMemberContactDataModel>
    {
        public GroupMemberContextMenu(ConnectViewModel connect) : base(connect) {}

        protected override void UpdateItemsFor(GroupMemberContactDataModel item) {
            UpdateForItemBase(item);

            GetAsyncItem(RemoveFromGroup)
                .IsVisible = item.Group.IsMine && !item.IsOwner;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public Task RemoveFromGroup(GroupMemberContactDataModel entity) {
            return Connect.RemoveGroupMember(entity);
        }
    }
}