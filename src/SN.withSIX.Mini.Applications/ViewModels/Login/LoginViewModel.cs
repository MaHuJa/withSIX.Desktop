// <copyright company="SIX Networks GmbH" file="LoginViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Applications.Usecases.Main;

namespace SN.withSIX.Mini.Applications.ViewModels.Login
{
    public class LoginViewModel : ScreenViewModel, ILoginViewModel
    {
        static readonly string displayName = Cheat.WindowDisplayName("Login");
        readonly Uri _callbackUri;

        public LoginViewModel(Uri uri, Uri callbackUri) {
            Uri = uri;
            _callbackUri = callbackUri;
            Nav = ReactiveCommand.CreateAsyncTask(HandleTask)
                .DefaultSetup("Login nav");
        }

        public Uri Uri { get; set; }
        public override string DisplayName => displayName;
        public IReactiveCommand<Unit> Nav { get; }

        public bool Navigating(Uri uri) {
#if DEBUG
            MainLog.Logger.Debug("LoginDialog navigating: " + uri);
#endif
            if (!uri.ToString().StartsWith(_callbackUri.AbsoluteUri))
                return false;
            Nav.Execute(uri);

            return true;
        }

        async Task HandleTask(object x) {
            // TODO: Combine commands
            var authorizeResponse = Cheat.Mediator.Request(new ProcessLoginCommand((Uri) x, _callbackUri));
            await
                Cheat.Mediator.RequestAsync(new PerformAuthentication(authorizeResponse.Code, _callbackUri))
                    .ConfigureAwait(false);
            Close.Execute(null);
        }
    }

    public interface ILoginViewModel : IScreenViewModel
    {
        IReactiveCommand<Unit> Nav { get; }
        Uri Uri { get; set; }
        bool Navigating(Uri uri);
    }
}