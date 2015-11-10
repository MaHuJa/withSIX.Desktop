// <copyright company="SIX Networks GmbH" file="RemoveUserFromGroupCommand.cs">
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
    public class RemoveUserFromGroupCommand : IAsyncRequest<UnitType>
    {
        public RemoveUserFromGroupCommand(Guid friendId, Guid groupId) {
            FriendId = friendId;
            GroupId = groupId;
        }

        public Guid FriendId { get; }
        public Guid GroupId { get; }
    }

    [StayPublic]
    public class RemoveUserFromGroupCommandHandler : IAsyncRequestHandler<RemoveUserFromGroupCommand, UnitType>
    {
        readonly IConnectApiHandler _apiHandler;
        readonly ContactList _contactList;

        public RemoveUserFromGroupCommandHandler(ContactList contactList, IConnectApiHandler apiHandler) {
            _contactList = contactList;
            _apiHandler = apiHandler;
        }

        public Task<UnitType> HandleAsync(RemoveUserFromGroupCommand request) {
            var group = _contactList.UserInfo.Groups.First(x => x.Id == request.GroupId);
            var user = group.Members.First(x => x.Id == request.FriendId);
            return _apiHandler.RemoveUserFromGroup(user, group).Void();
        }
    }
}