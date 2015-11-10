// <copyright company="SIX Networks GmbH" file="FriendServerChangedCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class FriendServerChangedCommand : IAsyncRequest<UnitType> {}

    public class FriendServerChangedCommandHandler : IAsyncRequestHandler<FriendServerChangedCommand, UnitType>
    {
        public async Task<UnitType> HandleAsync(FriendServerChangedCommand request) {
            throw new NotImplementedException();
        }
    }
}