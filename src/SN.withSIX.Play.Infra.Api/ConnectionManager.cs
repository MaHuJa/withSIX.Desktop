// <copyright company="SIX Networks GmbH" file="ConnectionManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using SignalRNetClientProxyMapper;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Api.Hubs;

namespace SN.withSIX.Play.Infra.Api
{
    class ConnectionManager : PropertyChangedBase, IDisposable, IConnectionManager, IInfrastructureService
    {
        const int MaxTries = 10;
        readonly HubConnection _connection;
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly object _startLock = new object();
        readonly TimerWithElapsedCancellationAsync _timer2;
        readonly ITokenRefresher _tokenRefresher;
        IAccountHub _accountHub;
        IApiHub _apiHub;
        IChatHub _chatHub;
        ICollectionsHub _collectionsHub;
        AccountInfo _context;
        IGroupHub _groupHub;
        bool _initialized;
        bool _isConnected;
        bool _isStopped;
        IMissionsHub _missionsHub;
        Task _startTask;
        int _tries;

        public ConnectionManager(Uri hubHost, ITokenRefresher tokenRefresher) {
            Contract.Requires<ArgumentNullException>(hubHost != null);
            _tokenRefresher = tokenRefresher;
            MessageBus = new MessageBus();
            _connection = new HubConnection(hubHost.ToString()) {JsonSerializer = CreateJsonSerializer()};
            _connection.Error += ConnectionOnError;
            _connection.StateChanged += ConnectionOnStateChanged;
            _connection.Closed += ConnectionClosed;
            _timer2 = new TimerWithElapsedCancellationAsync(new TimeSpan(0, 20, 0).TotalMilliseconds, RefreshTokenTimer);
        }

        public ConnectionState State
        {
            get { return _connection != null ? _connection.State : ConnectionState.Disconnected; }
            private set { OnPropertyChanged(); }
        }
        public IChatHub ChatHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _chatHub;
            }
            private set { _chatHub = value; }
        }
        public ICollectionsHub CollectionsHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _collectionsHub;
            }
            private set { _collectionsHub = value; }
        }
        public IAccountHub AccountHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _accountHub;
            }
            private set { _accountHub = value; }
        }
        public IGroupHub GroupHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _groupHub;
            }
            private set { _groupHub = value; }
        }
        public IMissionsHub MissionsHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _missionsHub;
            }
            private set { _missionsHub = value; }
        }
        public IApiHub ApiHub
        {
            get
            {
                Task.WaitAll(WaitForReconnect());
                return _apiHub;
            }
            private set { _apiHub = value; }
        }
        public IMessageBus MessageBus { get; }
        public string ApiKey { get; private set; }

        public AccountInfo Context() {
            var contextModel = DomainEvilGlobal.SecretData.UserInfo.Account;
            if (contextModel == null)
                throw new NotLoggedInException();
            return contextModel;
        }

        [Obsolete]
        public Task SetupContext() {
            _context = DomainEvilGlobal.SecretData.UserInfo.Account;
            return TaskExt.Default;
        }

        public bool IsLoggedIn() {
            return _context != null;
        }

        public bool IsConnected() {
            return _connection.State == ConnectionState.Connected;
        }

        public Task Start(string key = null) {
            return StartGTask(key);
        }

        public async Task Stop() {
            _isStopped = true;
            if (_connection.State == ConnectionState.Disconnected)
                return;
            await Task.Run(() => _connection.Stop()).ConfigureAwait(false);
        }

        public void Dispose() {
            if (_timer2 != null)
                _timer2.Dispose();
            if (_disposables != null)
                _disposables.Dispose();
            if (_connection != null)
                _connection.Dispose();
        }

        async Task<bool> RefreshTokenTimer() {
            if (_connection.State == ConnectionState.Connected && !ApiKey.IsBlankOrWhiteSpace())
                await RefreshToken().ConfigureAwait(false);
            return true;
        }

        Task StartGTask(string key = null) {
            lock (_startLock) {
                if (_startTask == null || _startTask.IsCompleted)
                    _startTask = StartInternal(key);
                return _startTask;
            }
        }

        async Task StartInternal(string key = null) {
            if (!_initialized) {
                SetupHubs();
                _initialized = true;
            }
            _isStopped = false;
            if (_connection.State != ConnectionState.Disconnected)
                await Stop().ConfigureAwait(false);

#if DEBUG
            MainLog.Logger.Debug("Trying to connect...");
#endif
            await RefreshToken().ConfigureAwait(false);

            try {
                var startTask = _connection.Start();

                await Task.WhenAny(startTask, Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);

                if (!startTask.IsCompleted)
                    throw new TimeoutException("SignalR Failed to connect in due time.");
            } catch (HttpClientException ex) {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    throw new UnauthorizedException("SignalR was not authorized", ex);
                throw;
            }
#if DEBUG
            MainLog.Logger.Debug("Connected...");
#endif
        }

        async Task RefreshToken() {
            var token = await _tokenRefresher.RefreshTokenTask().ConfigureAwait(false);
            if (token != ApiKey)
                SetConnectionKey(token);
        }

        async Task WaitForReconnect() {
            if (State == ConnectionState.Connected)
                return;
            if (State == ConnectionState.Connecting || State == ConnectionState.Reconnecting)
                await WaitForStateChange().ConfigureAwait(false);
        }

        async Task WaitForStateChange() {
            using (var cts = new CancellationTokenSource(new TimeSpan(0, 1, 0)))
            using (this.WhenAnyValue(x => x.State).Skip(1).Subscribe(x => cts.Cancel()))
                await Task.Delay(new TimeSpan(0, 1, 0), cts.Token).ConfigureAwait(false);
        }

        public void ClearContext() {
            _context = null;
        }

        static JsonSerializer CreateJsonSerializer() {
            return JsonSerializer.Create(SerializationExtension.DefaultSettings);
        }

        async Task<bool> HeartBeat() {
            if (_connection.State == ConnectionState.Connected && !ApiKey.IsBlankOrWhiteSpace())
                await AccountHub.Heartbeat().ConfigureAwait(false);
            return true;
        }

        async void ConnectionClosed() {
            if (!_isStopped) {
                if (_connection.State == ConnectionState.Disconnected && _tries < MaxTries) {
                    MessageBus.SendMessage(new ConnectionStateChanged(_isConnected) {
                        ConnectedState = ConnectedState.Connecting
                    });
                    await Task.Delay(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
                    await TryReconnecting().ConfigureAwait(false);
                } else {
                    MessageBus.SendMessage(new ConnectionStateChanged(_isConnected) {
                        ConnectedState = ConnectedState.ConnectingFailed
                    });
                }
            } else {
                MessageBus.SendMessage(new ConnectionStateChanged(_isConnected) {
                    ConnectedState = ConnectedState.Disconnected
                });
            }
        }

        async Task TryReconnecting() {
            try {
#if DEBUG
                MainLog.Logger.Debug("Trying to reconnect...");
#endif
                await _connection.Start().ConfigureAwait(false);
                if (!ApiKey.IsNullOrEmpty())
                    await SetupContext().ConfigureAwait(false);

#if DEBUG
                MainLog.Logger.Debug("Connected...");
#endif
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e, "Error during signalr reconnect");
                _tries += 1;
            }
        }

        void SetConnected(bool connected) {
            if (_isConnected == connected)
                return;
            _isConnected = connected;

            if (connected) {
                MessageBus.SendMessage(new ConnectionStateChanged(_isConnected) {
                    ConnectedState = ConnectedState.Connected
                });
            }
        }

        void ConnectionOnStateChanged(StateChange stateChange) {
            switch (stateChange.NewState) {
            case ConnectionState.Connecting:
                SetConnected(false);
                break;
            case ConnectionState.Connected:
                _tries = 0;
                SetConnected(true);
                break;
            case ConnectionState.Reconnecting:
                SetConnected(false);
                break;
            case ConnectionState.Disconnected: {
                ClearContext();
                SetConnected(false);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
            }
            State = stateChange.NewState;
        }

        void SetConnectionKey(string key) {
            key = key.IsBlankOrWhiteSpace() ? null : key;
            if (key == ApiKey)
                return;
            ApiKey = key;

            if (_connection.Headers.ContainsKey("Authorization"))
                _connection.Headers.Remove("Authorization");

            if (key != null)
                _connection.Headers.Add("Authorization", "Bearer " + key);
        }

        static void ConnectionOnError(Exception exception) {
            MainLog.Logger.FormattedWarnException(exception, "Error occurred in signalr connection");
        }

        void SetupHubs() {
            ChatHub = CreateHub<IChatHub>();
            AccountHub = CreateHub<IAccountHub>();
            GroupHub = CreateHub<IGroupHub>();
            MissionsHub = CreateHub<IMissionsHub>();
            CollectionsHub = CreateHub<ICollectionsHub>();
            ApiHub = CreateHub<IApiHub>();
        }

        THub CreateHub<THub>(HubConnection connection = null)
            where THub : class, IClientHubProxyBase {
            var con = connection ?? _connection;
            var hub = con.CreateStrongHubProxyWithExceptionUnwrapping<THub>();
            SubscribeToEvents(hub);
            return hub;
        }

        void SubscribeToEvents<T>(T strongHub)
            where T : IClientHubProxyBase {
            var functions = typeof (T).GetMethods().Where(x => x.ReturnType == typeof (IDisposable));
            foreach (var function in functions) {
                var parameterInfo = function.GetParameters()[0];
                var arguments = parameterInfo.ParameterType.GenericTypeArguments;
                this.CallGeneric<Action<MethodInfo, object>>(InvokeEventSubscriber<object>, arguments)(function,
                    strongHub);
            }
        }

        void InvokeEventSubscriber<TMessage>(MethodInfo function, object strongHub) {
            Action<TMessage> invokeAttr = InvokeEvent;
            _disposables.Add((IDisposable) function.Invoke(strongHub, new object[] {invokeAttr}));
        }

        void InvokeEvent<T>(T message) {
            MessageBus.SendMessage(message);
        }
    }
}