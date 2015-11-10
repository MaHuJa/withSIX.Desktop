using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.Usecases.Settings
{
    public class SaveLogs : IAsyncVoidCommand {}

    public class SaveLogsHandler : IAsyncVoidCommandHandler<SaveLogs>
    {
        readonly IRestarter _restarter;

        public SaveLogsHandler(IRestarter restarter) {
            _restarter = restarter;
        }

        public async Task<UnitType> HandleAsync(SaveLogs request) {
            var path =
                Common.Paths.TempPath.GetChildFileWithName("Sync diagnostics " + DateTime.UtcNow.ToFileTimeUtc() +
                                                           ".7z");
            await Common.GenerateDiagnosticZip(path).ConfigureAwait(false);
            Tools.FileUtil.SelectInExplorer(path.ToString());
            return UnitType.Default;
        }
    }

    public class StartInDiagnosticsMode : IAsyncVoidCommand {}

    public class StartInDiagnosticsModeHandler : IAsyncVoidCommandHandler<StartInDiagnosticsMode>
    {
        readonly IRestarter _restarter;

        public StartInDiagnosticsModeHandler(IRestarter restarter) {
            _restarter = restarter;
        }

        public async Task<UnitType> HandleAsync(StartInDiagnosticsMode request) {
            _restarter.RestartWithoutElevation(
                Tools.Generic.GetStartupParameters().Concat(new[] {"--verbose"}).ToArray());
            return UnitType.Default;
        }
    }
}