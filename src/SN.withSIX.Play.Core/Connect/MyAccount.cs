// <copyright company="SIX Networks GmbH" file="MyAccount.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public class MyAccountModel
    {
        public Account Account { get; set; }
        public int UnreadPrivateMessages { get; set; }
    }
        public class MyAccount : PropertyChangedBase
    {
        Account _account;
        int _unreadPrivateMessages;
        public Account Account
        {
            get { return _account; }
            set { SetProperty(ref _account, value); }
        }
        public int UnreadPrivateMessages
        {
            get { return _unreadPrivateMessages; }
            set { SetProperty(ref _unreadPrivateMessages, value); }
        }
    }
}