// <copyright company="SIX Networks GmbH" file="GetGeneralSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetGeneralSettings : IAsyncQuery<GeneralSettings> {}

    public class GetGeneralSettingsHandler : DbQueryBase, IAsyncRequestHandler<GetGeneralSettings, GeneralSettings>
    {
        public GetGeneralSettingsHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<GeneralSettings> HandleAsync(GetGeneralSettings request) {
            return SettingsContext.Settings.MapTo<GeneralSettings>();
        }
    }

    public class GeneralSettings
    {
        public bool LaunchWithWindows { get; set; }
        public bool OptOutErrorReports { get; set; }
        public bool EnableDesktopNotifications { get; set; }
    }
}