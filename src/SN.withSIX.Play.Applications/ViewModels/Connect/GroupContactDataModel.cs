// <copyright company="SIX Networks GmbH" file="GroupContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class GroupContactDataModel : ContactDataModel
    {
        readonly object _membersLock = new Object();
        IReactiveDerivedList<GroupMemberContactDataModel> _members;
        GroupMemberContactDataModel _selectedMember;

        public GroupContactDataModel(Group domainModel, ContactList contactList, ConnectViewModel connect)
            : base(domainModel, contactList, connect) {
            Group = domainModel;
            ConvertMembers();

            UiHelper.TryOnUiThread(() =>
                MembersView =
                    Members.SetupDefaultCollectionView(new[] {
                        new SortDescription("SortKey", ListSortDirection.Ascending),
                        new SortDescription("Model.DisplayName", ListSortDirection.Ascending)
                    }, null, null, null, true));

            this.WhenAnyValue(x => x.SelectedMember)
                .Where(x => x != null)
                .Subscribe(x => x.Selected());
        }

        public Group Group { get; }
        public ICollectionView MembersView { get; protected set; }
        public IReactiveDerivedList<GroupMemberContactDataModel> Members
        {
            get { return _members; }
            private set { SetProperty(ref _members, value); }
        }
        public GroupMemberContactDataModel SelectedMember
        {
            get { return _selectedMember; }
            set { SetProperty(ref _selectedMember, value); }
        }

        void ConvertMembers() {
            var members = Group.Members.CreateDerivedCollection(
                x =>
                    new GroupMemberContactDataModel(ContactList.FindFriend(x.Id) ?? new Friend(x), Group, ContactList,
                        Connect));
            UiHelper.TryOnUiThread(() => members.EnableCollectionSynchronization(_membersLock));
            Members = members;
        }

        public override void Selected() {
            Connect.GroupContextMenu.SetNextItem(this);
            var selectedMember = SelectedMember;
            if (selectedMember != null)
                selectedMember.Selected();
        }
    }
}