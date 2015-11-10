// <copyright company="SIX Networks GmbH" file="UserContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Concurrency;
using ReactiveUI;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class UserContactDataModel : ContactDataModel
    {
        public UserContactDataModel(Friend domainModel, ContactList contactList, ConnectViewModel connect)
            : base(domainModel, contactList, connect) {
            Friend = domainModel;
            _sortKey = Friend.WhenAnyValue(
                x => x.Status, state => CalculateSortKey((int) state))
                .ToProperty(this, x => x.SortKey, 0, Scheduler.Immediate);
        }

        public Friend Friend { get; }

        public override void Selected() {
            Connect.UserContactContextMenu.SetNextItem(this);
        }
    }
}