// <copyright company="SIX Networks GmbH" file="InviteRequest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Connect
{
    public class InviteRequest : ConnectModelBase, IContact
    {
        bool _isHidden;

        public InviteRequest(Guid id, bool isMe) : base(id) {
            IsMine = isMe;
        }

        public DateTime CreatedAt { get; set; }
        public Account Account { get; set; }
        public Account Target { get; set; }
        public bool IsMine { get; }
        public bool IsHidden
        {
            get { return _isHidden; }
            set { SetProperty(ref _isHidden, value); }
        }
        public string DisplayName
        {
            get { return IsMine ? Target.DisplayName : Account.DisplayName; }
        }

        public Uri GetUri() {
            return IsMine ? Target.GetUri() : Account.GetUri();
        }

        public Uri GetOnlineConversationUrl() {
            return Tools.Transfer.JoinUri(GetUri(), "messages");
        }
    }
}