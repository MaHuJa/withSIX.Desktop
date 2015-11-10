// <copyright company="SIX Networks GmbH" file="IConnectChatApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SN.withSIX.Play.Core.Connect.Infrastructure.Components
{
    [ContractClass(typeof (ConnectChatApiContract))]
    public interface IConnectChatApi
    {
        Task<GroupChat> GetGroupChat(Group group);
        Task<PrivateChat> GetPrivateChat(Friend friend);
        Task<PrivateMessage> SendPrivateChatMessage(ChatInput cm, PrivateChat privateChat);
        Task<ChatMessage> SendPublicChatMessage(ChatInput cm, PublicChat publicChat);
        Task<ChatMessage> SendGroupChatMessage(ChatInput cm, GroupChat groupChat);
        Task MarkAsRead(Account account);
    }

    [ContractClassFor(typeof (IConnectChatApi))]
    public abstract class ConnectChatApiContract : IConnectChatApi
    {
        public abstract Task<GroupChat> GetGroupChat(Group @group);
        public abstract Task<PrivateChat> GetPrivateChat(Friend friend);

        public Task<PrivateMessage> SendPrivateChatMessage(ChatInput cm, PrivateChat privateChat) {
            Contract.Requires<ArgumentNullException>(cm != null);
            Contract.Requires<ArgumentNullException>(privateChat != null);
            return default(Task<PrivateMessage>);
        }

        public Task<ChatMessage> SendPublicChatMessage(ChatInput cm, PublicChat publicChat) {
            Contract.Requires<ArgumentNullException>(cm != null);
            Contract.Requires<ArgumentNullException>(publicChat != null);
            return default(Task<ChatMessage>);
        }

        public Task<ChatMessage> SendGroupChatMessage(ChatInput cm, GroupChat groupChat) {
            Contract.Requires<ArgumentNullException>(cm != null);
            Contract.Requires<ArgumentNullException>(groupChat != null);
            return default(Task<ChatMessage>);
        }

        public abstract Task MarkAsRead(Account account);
    }
}