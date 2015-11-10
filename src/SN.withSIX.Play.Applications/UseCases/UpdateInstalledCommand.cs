// <copyright company="SIX Networks GmbH" file="UpdateInstalledCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class UpdateInstalledCommand : IAsyncRequest<UnitType> {}

    public class UpdateInstalledCommandHandler : IAsyncRequestHandler<UpdateInstalledCommand, UnitType>
    {
        public async Task<UnitType> HandleAsync(UpdateInstalledCommand request) {
            throw new NotImplementedException();
        }
    }
}