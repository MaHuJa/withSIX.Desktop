// <copyright company="SIX Networks GmbH" file="PremiumEventHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Applications.NotificationHandlers
{
    public class PremiumEventHandler : IAsyncNotificationHandler<TokenUpdatedEvent>
    {
        readonly IAuthProvider _authProvider;
        readonly IGameContext _context;
        readonly UserSettings _settings;

        public PremiumEventHandler(UserSettings settings, IGameContext context, IAuthProvider authProvider) {
            _settings = settings;
            _context = context;
            _authProvider = authProvider;
        }

        public Task HandleAsync(TokenUpdatedEvent notification) {
#if DEBUG
            if (notification.PremiumToken != null) {
                MainLog.Logger.Debug("Premium UN: " + notification.PremiumToken.AccessToken);
                MainLog.Logger.Debug("Premium Token: " + notification.PremiumToken.PremiumKey);
            } else
                MainLog.Logger.Debug("Not Premium");
#endif
            return notification.PremiumToken == null ? RemovePremium() : AddPremium(notification.PremiumToken);
        }

        async Task RemovePremium() {
            _settings.AccountOptions.SetP(false);
            foreach (var endpoint in Common.PremiumHosts)
                _authProvider.SetNonPersistentAuthInfo(("http://" + endpoint).ToUri(), null);

            foreach (var c in _context.Games.Select(x => x.Controller).Where(x => x != null))
                await c.RemovePremium().ConfigureAwait(false);
        }

        async Task AddPremium(PremiumAccessTokenV1 premiumToken) {
            _settings.AccountOptions.SetP(true);
            foreach (var endpoint in Common.PremiumHosts) {
                _authProvider.SetNonPersistentAuthInfo(("http://" + endpoint).ToUri(),
                    new AuthInfo(premiumToken.AccessToken, premiumToken.PremiumKey));
            }

            foreach (var c in _context.Games.Select(x => x.Controller).Where(x => x != null))
                await c.AddPremium().ConfigureAwait(false);
        }
    }
}