// <copyright company="SIX Networks GmbH" file="UpdateAvailableCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class UpdateAvailableCommand : IAsyncRequest<UnitType> {}

    public class UpdateAvailableCommandHandler :
        IAsyncRequestHandler<UpdateAvailableCommand, UnitType>
    {
        readonly ISoftwareUpdate _softwareUpdate;

        public UpdateAvailableCommandHandler(ISoftwareUpdate softwareUpdate) {
            _softwareUpdate = softwareUpdate;
        }

        public async Task<UnitType> HandleAsync(UpdateAvailableCommand request) {
            _softwareUpdate.UpdateAndExitIfNotBusy(false);
            return UnitType.Default;
        }
    }
}