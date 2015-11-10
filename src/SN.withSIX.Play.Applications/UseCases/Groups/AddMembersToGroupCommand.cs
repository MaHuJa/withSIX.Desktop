// <copyright company="SIX Networks GmbH" file="AddMembersToGroupCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases.Groups
{
    public class AddMembersToGroupCommand : IAsyncRequest<UnitType>
    {
        public AddMembersToGroupCommand(Guid groupId, Guid[] users) {
            GroupId = groupId;
            Users = users;
        }

        public Guid GroupId { get; }
        public Guid[] Users { get; }
    }


    [StayPublic]
    public class AddMembersToGroupCommandHandler : IAsyncRequestHandler<AddMembersToGroupCommand, UnitType>
    {
        readonly IConnectApiHandler _api;

        public AddMembersToGroupCommandHandler(IConnectApiHandler api) {
            _api = api;
        }

        public async Task<UnitType> HandleAsync(AddMembersToGroupCommand request) {
            // TODO: Mass fetch, Mass add?
            foreach (var u in request.Users) {
                await
                    _api.AddUserToGroup(await _api.GetAccount(u).ConfigureAwait(false),
                        await _api.GetGroup(request.GroupId));
            }

            return UnitType.Default;
        }
    }
}