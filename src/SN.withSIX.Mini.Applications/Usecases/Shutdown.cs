// <copyright company="SIX Networks GmbH" file="Shutdown.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Main;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public class Shutdown : IAsyncVoidCommand, IExcludeWriteLock {}

    public class ShutdownCommandHandler : IAsyncVoidCommandHandler<Shutdown>
    {
        readonly IShutdownHandler _shutdownHandler;
        readonly IContentInstallationService _contentInstallation;

        public ShutdownCommandHandler(IShutdownHandler shutdownHandler, IContentInstallationService contentInstallation) {
            _shutdownHandler = shutdownHandler;
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(Shutdown request) {
            _contentInstallation.Abort();
            // TODO: Call Abort Synchronization?
            _shutdownHandler.Shutdown();

            return UnitType.Default;
        }
    }
}