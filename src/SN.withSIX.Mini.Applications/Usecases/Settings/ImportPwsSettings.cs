using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Settings
{
    public class ImportPwsSettings : IAsyncVoidCommand
    {
        
    }

    public class ImportPwsSettingsHandler : DbCommandBase, IAsyncVoidCommandHandler<ImportPwsSettings>
    {
        readonly IPlayWithSixImporter _importer;
        public ImportPwsSettingsHandler(IDbContextLocator dbContextLocator, IPlayWithSixImporter importer) : base(dbContextLocator) {
            _importer = importer;
        }

        public async Task<UnitType> HandleAsync(ImportPwsSettings request) {
            var path = _importer.DetectPwSSettings();
            if (path == null)
                throw new ValidationException("PwS is not detected");
            await _importer.ImportPwsSettings(path).ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}