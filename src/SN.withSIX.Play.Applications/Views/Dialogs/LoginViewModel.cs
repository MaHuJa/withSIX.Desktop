// <copyright company="SIX Networks GmbH" file="LoginViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.UseCases;

namespace SN.withSIX.Play.Applications.Views.Dialogs
{
    public class LoginViewModel : DialogBase, IDontIC
    {
        readonly Uri _callbackUri;

        public LoginViewModel(Uri uri, Uri callbackUri) {
            Uri = uri;
            _callbackUri = callbackUri;

            Close = ReactiveCommand.Create();
            Close.Subscribe(x => TryClose(false));
            Nav = ReactiveCommand.CreateAsyncTask(HandleTask).DefaultSetup("Process Login");
        }

        public ReactiveCommand<Unit> Nav { get; }
        public ReactiveCommand<object> Close { get; }
        public Uri Uri { get; set; }

        async Task HandleTask(object x) {
            Close.Execute(null);
            // TODO: Combine commands
            var authorizeResponse = Common.App.Request(new ProcessLoginCommand((Uri) x, _callbackUri));
            await
                Common.App.Mediator.RequestAsync(new GetAuthorization(authorizeResponse.Code, _callbackUri))
                    .ConfigureAwait(false);
        }

        public bool Navigating(Uri uri) {
#if DEBUG
            MainLog.Logger.Debug("LoginDialog navigating: " + uri);
#endif
            if (uri == null || !uri.ToString().StartsWith(_callbackUri.AbsoluteUri))
                return false;
            Nav.Execute(uri);

            return true;
        }
    }
}