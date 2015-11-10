// <copyright company="SIX Networks GmbH" file="FriendServerAddressChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Api.Models;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public class FriendServerAddressChanged
    {
        public readonly ServerAddress Address;
        public readonly Friend Source;

        public FriendServerAddressChanged(ServerAddress address, Friend entity) {
            Source = entity;
            Address = address;
        }
    }
}