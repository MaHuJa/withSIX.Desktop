// <copyright company="SIX Networks GmbH" file="AbortCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class AbortCommand : IAsyncVoidCommand, IHaveId<Guid>
    {
        public AbortCommand(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class AbortAllCommand : IAsyncVoidCommand {}

    public class AbortCommandHandler : IAsyncVoidCommandHandler<AbortCommand>, IAsyncVoidCommandHandler<AbortAllCommand>
    {
        readonly IContentInstallationService _contentInstallation;

        public AbortCommandHandler(IContentInstallationService contentInstallation) {
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(AbortCommand request) {
            _contentInstallation.Abort(request.Id);
            return UnitType.Default;
        }

        public async Task<UnitType> HandleAsync(AbortAllCommand request) {
            _contentInstallation.Abort();
            return UnitType.Default;
        }
    }
}