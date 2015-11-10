using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class SetLogin : IAsyncVoidCommand
    {

        public SetLogin(string apiKey) {
            ApiKey = apiKey;
        }

        public string ApiKey { get; }
    }

    public class SetLoginHandler : DbCommandBase, IAsyncVoidCommandHandler<SetLogin>
    {
        public SetLoginHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
        public async Task<UnitType> HandleAsync(SetLogin request) {
            SettingsContext.Settings.Secure.Login.Authentication.AccessToken = request.ApiKey;
            // TODO: Now handle the AccountInfo + PremiumToken
            // TODO: Refresh token when webbrowser refreshes...
            await SettingsContext.SaveSettings();

            return UnitType.Default;
        }
    }
}