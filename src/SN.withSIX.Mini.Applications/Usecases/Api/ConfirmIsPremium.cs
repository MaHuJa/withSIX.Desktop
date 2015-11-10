// <copyright company="SIX Networks GmbH" file="ConfirmIsPremium.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases.Main;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class ConfirmIsPremium : IAsyncVoidCommand {}

    public class ConfirmIsPremiumHandler : DbCommandBase, IAsyncVoidCommandHandler<ConfirmIsPremium>
    {
        readonly ITokenRefresher _refresher;

        public ConfirmIsPremiumHandler(IDbContextLocator dbContextLocator, ITokenRefresher refresher)
            : base(dbContextLocator) {
            _refresher = refresher;
        }

        public async Task<UnitType> HandleAsync(ConfirmIsPremium request) {
            var loginInfo = SettingsContext.Settings.Secure.Login;
            if (loginInfo != null && loginInfo.IsPremium)
                return UnitType.Default;
            if (loginInfo == null || !loginInfo.IsLoggedIn) {
                // open login dialog..
                throw new NotLoggedinException();
            }
            await _refresher.RefreshTokenTask().ConfigureAwait(false);
            return UnitType.Default;
        }
    }

    public class NotLoggedinException : Exception {}
}