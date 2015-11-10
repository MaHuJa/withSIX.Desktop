// <copyright company="SIX Networks GmbH" file="LeaveGroupCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases.Groups
{
    public class LeaveGroupCommand : IAsyncRequest<UnitType>
    {
        public LeaveGroupCommand(Guid groupId) {
            GroupId = groupId;
        }

        public Guid GroupId { get; }
    }


    [StayPublic]
    public class LeaveGroupCommandHandler : IAsyncRequestHandler<LeaveGroupCommand, UnitType>
    {
        readonly IConnectApiHandler _api;

        public LeaveGroupCommandHandler(IConnectApiHandler api) {
            _api = api;
        }

        public Task<UnitType> HandleAsync(LeaveGroupCommand request) {
            return _api.LeaveGroup(_api.Me.Groups.First(x => x.Id == request.GroupId)).Void();
        }
    }
}