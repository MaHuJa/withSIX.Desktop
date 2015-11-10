// <copyright company="SIX Networks GmbH" file="AddFriend.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public class AddFriend : PropertyChangedBase
    {
        bool _isContact;
        bool _isMutualFriend;

        public AddFriend(Account account) {
            Account = account;
        }

        public Account Account { get; private set; }
        public bool IsContact
        {
            get { return _isContact; }
            set { SetProperty(ref _isContact, value); }
        }
        public bool IsMutualFriend
        {
            get { return _isMutualFriend; }
            set { SetProperty(ref _isMutualFriend, value); }
        }
    }
}