// <copyright company="SIX Networks GmbH" file="GroupMemberContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class GroupMemberContactDataModel : UserContactDataModel
    {
        bool _isOwner;

        public GroupMemberContactDataModel(Friend domainModel, Group group, ContactList contactList,
            ConnectViewModel connect) : base(domainModel, contactList, connect) {
            Group = group;
            IsOwner = domainModel.Id == Group.Owner.Id;
            IsMe = domainModel.Id == ContactList.UserInfo.Account.Id;
        }

        public Group Group { get; }
        public bool IsOwner
        {
            get { return _isOwner; }
            set { SetProperty(ref _isOwner, value); }
        }
        public bool IsMe { get; private set; }

        public override void Selected() {
            Connect.GroupMemberContextMenu.SetNextItem(this);
        }
    }
}