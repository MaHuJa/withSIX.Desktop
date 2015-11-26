using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class Login : IAsyncVoidCommand
    {
        public Login(AccessInfo info) {
            Info = info;
        }

        public AccessInfo Info { get; }
    }

    public class LoginHandler : DbCommandBase, IAsyncVoidCommandHandler<Login>
    {
        private readonly ITokenRefresher _tokenRefresher;

        public LoginHandler(IDbContextLocator dbContextLocator, ITokenRefresher tokenRefresher) : base(dbContextLocator) {
            _tokenRefresher = tokenRefresher;
        }

        public Task<UnitType> HandleAsync(Login request) {
            return _tokenRefresher.HandleLogin(request.Info).Void();
        }
    }
}