// <copyright company="SIX Networks GmbH" file="AddFriendToGroupCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases.Groups
{
    public class AddFriendToGroupCommand : IAsyncRequest<UnitType>
    {
        public AddFriendToGroupCommand(Guid friendId, Guid groupId) {
            FriendId = friendId;
            GroupId = groupId;
        }

        public Guid FriendId { get; }
        public Guid GroupId { get; }
    }

    [StayPublic]
    public class AddFriendToGroupCommandHandler : IAsyncRequestHandler<AddFriendToGroupCommand, UnitType>
    {
        readonly IConnectApiHandler _apiHandler;
        readonly ContactList _contactList;

        public AddFriendToGroupCommandHandler(ContactList contactList, IConnectApiHandler apiHandler) {
            _contactList = contactList;
            _apiHandler = apiHandler;
        }

        public Task<UnitType> HandleAsync(AddFriendToGroupCommand request) {
            var friend = _contactList.FindFriend(request.FriendId);
            var group = _contactList.UserInfo.Groups.First(x => x.Id == request.GroupId);
            return _apiHandler.AddUserToGroup(friend.Account, group).Void();
        }
    }
}