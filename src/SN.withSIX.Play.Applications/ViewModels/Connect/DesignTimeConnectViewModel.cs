// <copyright company="SIX Networks GmbH" file="DesignTimeConnectViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class DesignTimeConnectViewModel : ConnectViewModel, IDesignTimeViewModel
    {
        public DesignTimeConnectViewModel()
            : base(
                IoC.Get<ContactList>(),
                null, null, null, null, null, null, null,
                IoC.Get<UserSettings>(),
                IoC.Get<IEventAggregator>()) {
            var act = new Account(new Guid()) {DisplayName = "test123"};
            ContactList.UserInfo.Friends.Add(new Friend(act));

            var chat = new GroupChat(new Guid()) {
                Loaded = true,
                Messages =
                    new ReactiveList<ChatMessage> {
                        new ChatMessage(new Guid()) {Body = "Test message", Author = act}
                    },
                Users = new ReactiveList<Account> {act}
            };
            Chats.Add(chat);
            ContactList.ActiveChat = chat;
            var grp = new Group(new Guid()) {Name = "Mon test grouppe"};
            grp.Members.Add(new Account(new Guid()) {DisplayName = "Test123"});
            ContactList.UserInfo.Groups.Add(grp);
            ContactList.LoginState = LoginState.LoggedIn;
            ContactList.ConnectedState = ConnectedState.Connected;
        }
    }
}