// <copyright company="SIX Networks GmbH" file="PerformUpdate.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class PerformUpdate : IAsyncVoidCommand {}

    public class PerformUpdateHandler : IAsyncVoidCommandHandler<PerformUpdate>
    {
        readonly IRestarter _restarter;
        readonly ISquirrelUpdater _squirrel;

        public PerformUpdateHandler(ISquirrelUpdater squirrel, IRestarter restarter) {
            _squirrel = squirrel;
            _restarter = restarter;
        }

        public async Task<UnitType> HandleAsync(PerformUpdate request) {
            // TODO: Progress reporting etc
            await new AppStateUpdated(AppUpdateState.Updating).RaiseEvent().ConfigureAwait(false);
            var updateApp = await _squirrel.UpdateApp(p => { }).ConfigureAwait(false);

            _restarter.RestartInclEnvironmentCommandLine();
            return UnitType.Default;
        }
    }
}