// <copyright company="SIX Networks GmbH" file="ChatMessage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Connect
{
    public class ChatMessage : ConnectModelBase
    {
        string _body;
        DateTime _createdAt;
        bool _isMyMessage;
        bool _isUnread;
        public ChatMessage(Guid id) : base(id) {}
        public Account Author { get; set; }
        public string Body
        {
            get { return _body; }
            set { SetProperty(ref _body, value); }
        }
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { SetProperty(ref _createdAt, value); }
        }
        public bool IsUnread
        {
            get { return _isUnread; }
            set { SetProperty(ref _isUnread, value); }
        }
        public bool IsMyMessage
        {
            get { return _isMyMessage; }
            set { SetProperty(ref _isMyMessage, value); }
        }

        public bool ComparePK(ChatMessage other) {
            return other != null && ComparePK((ConnectModelBase) other);
        }
    }
}