// <copyright company="SIX Networks GmbH" file="PrivateMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Connect
{
    public class PrivateMessage : ChatMessage
    {
        public PrivateMessage(Guid id) : base(id) {}
        public Account Receiver { get; set; }
    }
}