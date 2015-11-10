// <copyright company="SIX Networks GmbH" file="SaveGeneralSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class SaveGeneralSettings : IAsyncVoidCommand
    {
        public GeneralSettings Settings { get; set; }
    }

    public class SaveGeneralSettingsHandler : DbCommandBase, IAsyncVoidCommandHandler<SaveGeneralSettings>
    {
        public SaveGeneralSettingsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<UnitType> HandleAsync(SaveGeneralSettings request) {
            request.Settings.MapTo(SettingsContext.Settings);
            await SettingsContext.SaveSettings().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}