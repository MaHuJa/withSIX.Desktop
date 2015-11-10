// <copyright company="SIX Networks GmbH" file="ChatContactDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class ChatContactDataModel : ContactDataModel
    {
        public ChatContactDataModel(PublicChat domainModel, ContactList contactList, ConnectViewModel connect)
            : base(domainModel, contactList, connect) {
            Chat = domainModel;
        }

        public PublicChat Chat { get; private set; }
        public override void Selected() {}
    }
}