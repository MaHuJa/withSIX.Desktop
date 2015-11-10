// <copyright company="SIX Networks GmbH" file="MessageContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using System.Windows;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class MessageContextMenu : ContextMenuBase<ChatMessage>
    {
        readonly ConnectViewModel _connect;

        public MessageContextMenu(ConnectViewModel connect) {
            _connect = connect;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon), DoNotObfuscate]
        public void CopyMessageToClipboard(ChatMessage message) {
            Clipboard.SetText(message.Body);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Joystick), DoNotObfuscate]
        public Task JoinServer(ChatMessage message) {
            return _connect.JoinServer(_connect.ContactList.FindFriend(message.Author));
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Add), DoNotObfuscate]
        public Task AddFriend(ChatMessage message) {
            return _connect.AddFriend(message.Author);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void RemoveFriend(ChatMessage message) {
            _connect.RemoveContact(_connect.ContactList.FindFriend(message.Author));
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Chat_Message), DoNotObfuscate]
        public void ReplyToUser(ChatMessage message) {
            _connect.Reply(message.Author);
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_Hexagon_Info), DoNotObfuscate]
        public void VisitProfile(ChatMessage message) {
            _connect.VisitProfile(message.Author);
        }

        protected override void UpdateItemsFor(ChatMessage item) {
            var isFriend = _connect.ContactList.IsFriend(item.Author.Id);
            GetAsyncItem(JoinServer)
                .IsVisible = isFriend;
            GetAsyncItem(AddFriend)
                .IsVisible = !isFriend && !_connect.ContactList.IsMe(item.Author) &&
                             !_connect.ContactList.HasInviteRequest(item.Author.Id);
            GetItem(RemoveFriend)
                .IsVisible = isFriend;

            if (isFriend) {
                var friend = _connect.ContactList.FindFriend(item.Author.Id);
                GetAsyncItem(JoinServer)
                    .IsEnabled = friend.PlayingOn != null;
            }
        }
    }
}