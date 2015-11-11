// <copyright company="SIX Networks GmbH" file="ContactList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Options;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;

namespace SN.withSIX.Play.Applications.Services
{
    [DoNotObfuscate]
    public class ContactList : PropertyChangedBase, IHandle<GameLaunchedEvent>, IHandle<GameTerminated>,
        IHandle<ApiKeyUpdated>,
        IHandle<RefreshLoginRequest>,
        IHandle<ConnectionStateChanged>,
        IEnableLogging, IDomainService
    {
        readonly IConnectApiHandler _apiHandler;
        readonly object _disposableLock = new object();
        readonly IEventAggregator _eventBus;
        readonly IMediator _mediator;
        readonly UserSettings _settings;
        ConnectedState _connectedState = ConnectedState.Disconnected;
        CompositeDisposable _disposables;
        bool _initialConnect;
        LoginState _loginState;
        bool _notificationsSet;
        OnlineStatus _onlineStatus = OnlineStatus.Online;
        DateTime _synchronizedAt;

        public ContactList(IEventAggregator ea,
            IConnectApiHandler handler, IMediator mediator,
            UserSettings settings) {
            _eventBus = ea;
            _mediator = mediator;
            _settings = settings;
            _apiHandler = handler;
            _apiHandler.MessageBus.Listen<ConnectionStateChanged>().Subscribe(Handle);

            LoginState = string.IsNullOrWhiteSpace(_settings.AccountOptions.AccessToken)
                ? LoginState.LoggedOut
                : LoginState.LoggedIn;

            this.WhenAnyValue(x => x.LoginState)
                .Skip(1)
                .Subscribe(HandleNewLogginState);

            // TODO: Not good
            /*
            _apiHandler.MessageBus.Listen<ConnectionStateChanged>()
                .Subscribe(x => {
                    if (LoginState == LoginState.LoggedIn)
                        ConnectedState = x.IsConnected ? ConnectedState.Connected : ConnectedState.Connecting;
                });
             */
        }

        public ConnectedState ConnectedState
        {
            get { return _connectedState; }
            set
            {
                if (_connectedState == value)
                    return;
                _connectedState = value;
                OnPropertyChanged();
            }
        }
        public LoginState LoginState
        {
            get { return _loginState; }
            set
            {
                if (_loginState == value)
                    return;
                _loginState = value;
                OnPropertyChanged();
            }
        }
        public MyAccount UserInfo => _apiHandler.Me;
        public OnlineStatus OnlineStatus
        {
            get { return _onlineStatus; }
            set { SetProperty(ref _onlineStatus, value); }
        }
        public DateTime SynchronizedAt
        {
            get { return _synchronizedAt; }
            set { SetProperty(ref _synchronizedAt, value); }
        }

        public async void Handle(ApiKeyUpdated message) {
            LoginState = string.IsNullOrWhiteSpace(message.ApiKey) ? LoginState.LoggedOut : LoginState.LoggedIn;
            await RefreshConnection().ConfigureAwait(false);
        }

        public void Handle(ConnectionStateChanged message) {
            if (message.ConnectedState == ConnectedState.Connected && _initialConnect)
                return;
            ConnectedState = message.ConnectedState;
        }

        public void Handle(GameLaunchedEvent message) {
            _eventBus.PublishOnCurrentThread(
                new MyActiveServerAddressChanged(message.Server != null ? message.Server.Address : null));
        }

        public void Handle(GameTerminated message) {
            _eventBus.PublishOnCurrentThread(new MyActiveServerAddressChanged(null));
        }

        public async void Handle(RefreshLoginRequest message) {
            await RefreshConnection().ConfigureAwait(false);
        }

        async Task RefreshConnection() {
            ConnectedState = ConnectedState.Disconnected;
            await HandleConnection().ConfigureAwait(false);
        }

        void HandleNewLogginState(LoginState loginState) {
            DomainEvilGlobal.Settings.RaiseChanged();
            if (loginState != LoginState.LoggedIn)
                ClearLists();
        }

        public async Task HandleConnection() {
            _initialConnect = true;
            try {
                await TryConnect().ConfigureAwait(false);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
            _initialConnect = false;
        }

        [SmartAssembly.ReportUsage.ReportUsage]
        public void RetrieveApiKey() {
            //Tools.OpenUrl(API_KEY_URL);
            _eventBus.PublishOnCurrentThread(new RequestOpenLogin());
        }

        public bool IsMe(Account user) {
            return IsMe(user.Id);
        }

        bool IsMe(Guid uuid) {
            var me = UserInfo;
            return me != null && uuid == me.Account.Id;
        }

        static bool VisitProfile(Uri uri) {
            return BrowserHelper.TryOpenUrlIntegrated(uri);
        }

        async Task TryConnect() {
            var apiKey = _settings.AccountOptions.AccessToken;
            ConnectedState = ConnectedState.Connecting;
            var isLoggedIn = !apiKey.IsBlankOrWhiteSpace();

            // TODO: Deal with Disconnect on logout, and Connect on relogin etc.
            try {
                await _apiHandler.Initialize(apiKey).ConfigureAwait(false);
                _settings.AccountOptions.AccountId = isLoggedIn ? _apiHandler.Me.Account.Id : Guid.Empty;
            } catch (UnauthorizedException ex) {
                this.Logger().FormattedWarnException(ex);
                ConnectedState = ConnectedState.ConnectingFailed;
                _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.LoginUrl));
                return;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                ConnectedState = ConnectedState.ConnectingFailed;
                throw;
            }

            ConnectedState = ConnectedState.Connected;

            if (!isLoggedIn) {
                ClearLists();
                LoginState = LoginState.LoggedOut;
                return;
            }

            LoginState = LoginState.LoggedIn;
            SetupHandlers();
            SynchronizedAt = Tools.Generic.GetCurrentUtcDateTime;
            //await SetServerAddress(ActiveServerAddress).ConfigureAwait(false);

            await _mediator.NotifyAsync(new LoggedInEvent()).ConfigureAwait(false);
        }

        void SetupHandlers() {
            if (_notificationsSet)
                return;

            lock (_disposableLock) {
                HandleDisposables();

                _disposables = new CompositeDisposable();
                // Not using collection initializer because otherwise we might end up with some subscriptions in limbo, when later subscription calls fail...

                _notificationsSet = true;
            }
        }

        void HandleDisposables() {
            lock (_disposableLock) {
                var disposables = _disposables;
                if (disposables == null)
                    return;
                disposables.Dispose();
                _disposables = null;
                _notificationsSet = false;
            }
        }

        void ClearLists() {
            HandleDisposables();
        }
    }
}