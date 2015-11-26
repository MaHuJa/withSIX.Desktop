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
using ReactiveUI;
using SignalRNetClientProxyMapper;
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
        readonly HubConnection _connection;
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly object _startLock = new object();
        bool _initialized;
        bool _isConnected;
        Task _startTask;

        public ConnectionManager(Uri hubHost) {
            Contract.Requires<ArgumentNullException>(hubHost != null);
            MessageBus = new MessageBus();
            _connection = new HubConnection(hubHost.ToString()) {JsonSerializer = CreateJsonSerializer()};
            _connection.Error += ConnectionOnError;
            _connection.StateChanged += ConnectionOnStateChanged;
        }

        public ConnectionState State
        {
            get { return _connection?.State ?? ConnectionState.Disconnected; }
            private set { OnPropertyChanged(); }
        }
        public ICollectionsHub CollectionsHub { get; private set; }
        public IMissionsHub MissionsHub { get; private set; }
        public IMessageBus MessageBus { get; }
        public string ApiKey { get; private set; }

        public bool IsLoggedIn() {
            return DomainEvilGlobal.SecretData.UserInfo.AccessToken != null;
        }

        public bool IsConnected() {
            return _connection.State == ConnectionState.Connected;
        }

        public Task Start(string key = null) {
            return StartGTask(key);
        }

        public async Task Stop() {
            if (_connection.State == ConnectionState.Disconnected)
                return;
            await Task.Run(() => _connection.Stop()).ConfigureAwait(false);
        }

        public void Dispose() {
            _disposables?.Dispose();
            _connection?.Dispose();
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
            if (_connection.State != ConnectionState.Disconnected)
                await Stop().ConfigureAwait(false);

#if DEBUG
            MainLog.Logger.Debug("Trying to connect...");
#endif

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

        static JsonSerializer CreateJsonSerializer() {
            return JsonSerializer.Create(SerializationExtension.DefaultSettings);
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
            MissionsHub = CreateHub<IMissionsHub>();
            CollectionsHub = CreateHub<ICollectionsHub>();
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