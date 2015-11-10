// <copyright company="SIX Networks GmbH" file="ContactList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public class LoggedInEvent {}

    public class FriendOnline
    {
        public FriendOnline(Friend friend) {
            Friend = friend;
        }

        public Friend Friend { get; private set; }
    }

    public class ChatInput : PropertyChangedBase
    {
        string _body;
        public string Body
        {
            get { return _body; }
            set { SetProperty(ref _body, value); }
        }
    }


    public class FriendRemoved
    {
        public readonly Friend Friend;

        public FriendRemoved(Friend friend) {
            Friend = friend;
        }
    }

    public class FriendAdded
    {
        public readonly Friend Friend;

        public FriendAdded(Friend friend) {
            Friend = friend;
        }
    }

    public class InviteRequestRemoved
    {
        public readonly InviteRequest Request;

        public InviteRequestRemoved(InviteRequest request) {
            Request = request;
        }
    }

    public class InviteRequestReceived
    {
        public readonly InviteRequest Request;

        public InviteRequestReceived(InviteRequest request) {
            Request = request;
        }
    }

    public class EntityUnreadMessagesChanged
    {
        public EntityUnreadMessagesChanged(Friend friend, int count) {
            Friend = friend;
            Count = count;
        }

        public int Count { get; private set; }
        public Friend Friend { get; private set; }
    }
}