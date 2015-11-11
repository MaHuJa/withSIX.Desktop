// <copyright company="SIX Networks GmbH" file="HomeView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using CefSharp;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.Views;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Infra.Api;

namespace SN.withSIX.Play.Presentation.Wpf.Views
{
    [DoNotObfuscate]
    public partial class HomeView : UserControl, IEnableLogging, IHandle<ApiKeyUpdated>, IHandle<DoLogout>, IHomeView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (HomeViewModel), typeof (HomeView),
                new PropertyMetadata(null));
        readonly ITokenRefresher _tokenRefresher;

        public HomeView(BrowserInterop browserInterop, ITokenRefresher tokenRefresher, IConnectApiHandler connMan) {
            InitializeComponent();
            _tokenRefresher = tokenRefresher;

            WebControl.LifeSpanHandler = new LifeSpanHandler();

            WebControl.RegisterJsObject("six_client", new Handler(browserInterop, connMan, GetAccessToken));

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.WhenAnyValue(x => x.ViewModel)
                    .Skip(1)
                    .Subscribe(vm => {
                        d(WebControl.WhenAnyValue(x => x.IsLoading).Subscribe(x => {
                            ViewModel.ProgressState.Active = x;
                            ViewModel.CanGoBack = WebControl.CanGoBack;
                            ViewModel.CanGoForward = WebControl.CanGoForward;
                        }));
                        d(vm.Navigate.Subscribe(x => {
                            switch (x) {
                            case HomeViewModel.NavigateMode.GoBack: {
                                WebControl.BackCommand.Execute(null);
                                break;
                            }
                            case HomeViewModel.NavigateMode.Abort: {
                                WebControl.Stop();
                                break;
                            }
                            case HomeViewModel.NavigateMode.GoForward: {
                                WebControl.ForwardCommand.Execute(null);
                                break;
                            }
                            case HomeViewModel.NavigateMode.Reload: {
                                WebControl.Reload(false);
                                break;
                            }
                            }
                        }));
                    }));

                d(this.WhenAnyValue(v => v.WebControl.IsLoading)
                    .Skip(1)
                    .Subscribe(x => ViewModel.IsNavigating = x));
            });

            CommandBindings.Add(new CommandBinding(BrowserView.CopyToClipboard, OnCopyToClipboard, CanCopyToClipboard));
            CommandBindings.Add(new CommandBinding(BrowserView.OpenInSystemBrowser, OnOpenInSystemBrowser,
                CanOpenInSystemBrowser));
        }

        public void Handle(DoLogout message) {
            Logout();
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as HomeViewModel; }
        }
        public HomeViewModel ViewModel
        {
            get { return (HomeViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        void CanOpenInSystemBrowser(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void OnOpenInSystemBrowser(object sender, ExecutedRoutedEventArgs e) {
            Tools.Generic.TryOpenUrl(WebControl.Address);
        }

        void OnCopyToClipboard(object sender, ExecutedRoutedEventArgs e) {
            Clipboard.SetText(WebControl.Address);
        }

        void CanCopyToClipboard(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        string GetAccessToken() {
            if (DomainEvilGlobal.SecretData.UserInfo.AccessToken == null)
                return null;
            if (!_tokenRefresher.Loaded)
                _tokenRefresher.RefreshTokenTask().Wait();
            return DomainEvilGlobal.SecretData.UserInfo.AccessToken;
        }

        class Handler
        {
            readonly BrowserInterop _interop;
            readonly IConnectApiHandler _connMan;

            public Handler(BrowserInterop interop, IConnectApiHandler connMan, Func<string> getAccessToken) {
                GetAccessToken = getAccessToken;
                _interop = interop;
                _connMan = connMan;
            }

            public Func<string> GetAccessToken { get; }

            public void subscribedToCollection(Guid id)
            {
                try
                {
                    _connMan.MessageBus.SendMessage(new SubscribedToCollection(id));
                }
                catch (Exception ex)
                {
                    MainLog.Logger.FormattedWarnException(ex, "error during JS exec");
                }
            }

            public void unsubscribedFromCollection(Guid id)
            {
                try
                {
                    _connMan.MessageBus.SendMessage(new UnsubscribedFromCollection(id));
                }
                catch (Exception ex)
                {
                    MainLog.Logger.FormattedWarnException(ex, "error during JS exec");
                }
            }

            public void open_pws_uri(string argument) {
                try {
                    _interop.OpenPwsUri(argument);
                } catch (Exception ex) {
                    MainLog.Logger.FormattedWarnException(ex, "error during JS exec");
                }
            }

            public void login() {
                try {
                    _interop.Login();
                } catch (Exception ex) {
                    MainLog.Logger.FormattedWarnException(ex, "error during JS exec");
                }
            }

            public void refresh_login() {
                try {
                    _interop.RefreshLogin();
                } catch (Exception ex) {
                    MainLog.Logger.FormattedWarnException(ex, "error during JS exec");
                }
            }

            public string get_api_key() {
                try {
                    return GetAccessToken();
                } catch (Exception ex) {
                    return null;
                }
            }
        }

        #region IHandle events

        public void Handle(ApiKeyUpdated message) {
            if (!string.IsNullOrWhiteSpace(message.ApiKey))
                Reload();
        }

        void Reload() {
            UiHelper.TryOnUiThread(() => WebControl.Reload(false));
        }

        void Logout() {
            UiHelper.TryOnUiThread(() => {
                try {
                    WebControl.ExecuteScriptAsync("$('form#logoutForm').submit();");
                } catch (InvalidOperationException ex) {
                    MainLog.Logger.FormattedWarnException(ex, "Could not logout of the webpage");
                }
            });
        }

        #endregion
    }

    public class LifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
            string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IWindowInfo windowInfo,
            ref bool noJavascriptAccess,
            out IWebBrowser newBrowser) {
            windowInfo.X = 640;
            windowInfo.Y = 640;

            newBrowser = null;
            return false;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) {}

        public bool DoClose(IWebBrowser browserControl, IBrowser browser) {
            return false;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) {}
    }
}