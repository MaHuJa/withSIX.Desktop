// <copyright company="SIX Networks GmbH" file="InviteRequestContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class InviteRequestContactDataModel : ContactDataModel
    {
        public InviteRequestContactDataModel(IContact domainModel, ContactList contactList, ConnectViewModel connect)
            : base(domainModel, contactList, connect) {}

        public override void Selected() {}
    }
}