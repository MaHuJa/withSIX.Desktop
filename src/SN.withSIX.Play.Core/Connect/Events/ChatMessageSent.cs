// <copyright company="SIX Networks GmbH" file="ChatMessageSent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public class ChatMessageSent : EventArgs
    {
        public ChatMessage ChatMessage;

        public ChatMessageSent(ChatMessage message) {
            Contract.Requires<ArgumentNullException>(message != null);
            ChatMessage = message;
        }
    }
}