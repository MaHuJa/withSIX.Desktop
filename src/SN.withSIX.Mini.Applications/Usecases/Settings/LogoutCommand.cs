// <copyright company="SIX Networks GmbH" file="LogoutCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases.Main;

namespace SN.withSIX.Mini.Applications.Usecases.Settings
{
    public class LogoutCommand : IAsyncVoidCommand {}

    public class LogoutCommandHandler : DbCommandBase, IAsyncVoidCommandHandler<LogoutCommand>
    {
        public LogoutCommandHandler(IDbContextLocator locator) : base(locator) {}

        public async Task<UnitType> HandleAsync(LogoutCommand request) {
            // TODO: Usecase implementation
            // TODO: Consider if we actually want to touch this directly even when not having clicked the OK button?
            SettingsContext.Settings.Secure.Login = null;

            await SettingsContext.SaveSettings().ConfigureAwait(false);
            // TODO: raise in domain somehow..
            await new LoginChanged(LoginInfo.Default).RaiseEvent().ConfigureAwait(false);

            return UnitType.Default;
        }
    }
}