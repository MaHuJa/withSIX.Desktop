// <copyright company="SIX Networks GmbH" file="ChatMessageUpdateReceived.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public class ChatMessageUpdateReceived : EventArgs
    {
        public Chat Chat;
        public ChatMessage ChatMessage;

        public ChatMessageUpdateReceived(Chat chat, ChatMessage message) {
            Contract.Requires<ArgumentNullException>(chat != null);
            Contract.Requires<ArgumentNullException>(message != null);

            Chat = chat;
            ChatMessage = message;
        }
    }
}