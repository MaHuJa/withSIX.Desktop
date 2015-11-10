// <copyright company="SIX Networks GmbH" file="ChatMessageReceived.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public abstract class ChatMessageReceived : EventArgs
    {
        public readonly IChat Chat;
        public readonly ChatMessage ChatMessage;

        protected ChatMessageReceived(IChat chat, ChatMessage message) {
            Contract.Requires<ArgumentNullException>(chat != null);
            Contract.Requires<ArgumentNullException>(message != null);

            Chat = chat;
            ChatMessage = message;
        }
    }

    public class PublicChatMessageReceived : ChatMessageReceived, IDomainEvent
    {
        public PublicChatMessageReceived(IChat chat, ChatMessage message) : base(chat, message) {}
    }

    public class GroupChatMessageReceived : ChatMessageReceived, IDomainEvent
    {
        public GroupChatMessageReceived(GroupChat chat, ChatMessage message) : base(chat, message) {
            GroupChat = chat;
        }

        public GroupChat GroupChat { get; private set; }
    }

    public class PrivateMessageReceived : ChatMessageReceived, IDomainEvent
    {
        public PrivateMessageReceived(PrivateChat chat, ChatMessage message) : base(chat, message) {
            PrivateChat = chat;
        }

        public PrivateChat PrivateChat { get; private set; }
    }
}