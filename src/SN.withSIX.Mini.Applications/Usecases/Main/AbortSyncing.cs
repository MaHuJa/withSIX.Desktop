// <copyright company="SIX Networks GmbH" file="AbortSyncing.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class AbortSyncing : IAsyncVoidCommand, IExcludeWriteLock {}

    public interface IExcludeWriteLock {}

    public class AbortSyncingHandler : IAsyncVoidCommandHandler<AbortSyncing>
    {
        readonly IContentInstallationService _contentInstallation;

        public AbortSyncingHandler(IContentInstallationService contentInstallation) {
            _contentInstallation = contentInstallation;
        }

        public async Task<UnitType> HandleAsync(AbortSyncing request) {
            // TODO: We need to cancel the syncing process etc, however since we pass CT (not CTS) into ContentInstallationService it is not possible atm.
            // Either we need to add a different CTS into the mix (in the contentinstallationservice), or we need to get the original CTS from somewhere?

            // I guess we can keep the original CTS for 'localized' abort, and a general CTS for overall abort?

            _contentInstallation.Abort();
            return UnitType.Default;
        }
    }
}