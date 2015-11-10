// <copyright company="SIX Networks GmbH" file="ChatContextMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class ChatContextMenu : ContextMenuBase<ChatViewModel>
    {
        readonly ConnectViewModel _connect;

        public ChatContextMenu(ConnectViewModel connect) {
            _connect = connect;
        }

        [MenuItem(Icon = SixIconFont.withSIX_icon_X), DoNotObfuscate]
        public void LeaveChat(ChatViewModel entity) {
            _connect.LeaveChat(entity);
        }
    }
}