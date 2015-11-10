// <copyright company="SIX Networks GmbH" file="DeleteGroupCommand.cs">
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
    public class DeleteGroupCommand : IAsyncRequest<UnitType>
    {
        public DeleteGroupCommand(Guid id) {
            ID = id;
        }

        public Guid ID { get; }
    }

    [StayPublic]
    public class DeleteGroupCommandHandler : IAsyncRequestHandler<DeleteGroupCommand, UnitType>
    {
        readonly IConnectApiHandler _api;

        public DeleteGroupCommandHandler(IConnectApiHandler api) {
            _api = api;
        }

        public Task<UnitType> HandleAsync(DeleteGroupCommand request) {
            return _api.DeleteGroup(_api.Me.Groups.First(x => x.Id == request.ID)).Void();
        }
    }
}