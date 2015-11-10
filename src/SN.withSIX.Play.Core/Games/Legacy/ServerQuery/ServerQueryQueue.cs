// <copyright company="SIX Networks GmbH" file="ServerQueryQueue.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class ServerQueryQueue : PropertyChangedBase, IServerQueryQueue, IEnableLogging
    {
        const int StartPort = 50000;
        readonly bool _useRangedEndPoints;
        ConcurrentQueue<IPEndPoint> _endpoints;
        int _lastPort;
        IServerQueryOverallState _state;

        public ServerQueryQueue(bool useRangedEndpoints = true) {
            State = new ServerQueryOverallState();
            _useRangedEndPoints = useRangedEndpoints;
        }

        public IServerQueryOverallState State
        {
            get { return _state; }
            private set { SetProperty(ref _state, value); }
        }

        public Task SyncAsync(Server[] objects) {
            return Sync(objects, default(CancellationToken));
        }

        public virtual Task SyncAsync(Server[] objects, CancellationToken token) {
            return Sync(objects, token);
        }

        Task Sync(ICollection<Server> objects, CancellationToken token) {
            State = new ServerQueryOverallState {Maximum = objects.Count, UnProcessed = objects.Count};

            return Enumerable.Empty<ServerQueryState>().SimpleRunningQueueAsync(1,
                blockingCollection => SyncServers(objects, token, state => {
                    if (!state.Success) {
                        state.Server.IsUpdating = false;
                        return;
                    }
                    blockingCollection.Add(state);
                }),
                x => {
                    TryUpdateServerFromSyncInfo(x);
                    State.IncrementProcessed();
                });
        }

        static void TryUpdateServerFromSyncInfo(ServerQueryState item) {
            try {
                item.Server.UpdateInfoFromResult(item.Result);
            } catch {
                item.Server.IsUpdating = false;
                throw;
            }
        }

        Task SyncServers(IEnumerable<Server> objects, CancellationToken token = default(CancellationToken),
            Action<ServerQueryState> callback = null) {
            if (_useRangedEndPoints)
                SetupEndPoints();
            return TrySyncServers(objects, token, callback);
        }

        void SetupEndPoints() {
            var p = StartPort;
            _endpoints =
                new ConcurrentQueue<IPEndPoint>(
                    Enumerable.Range(1, DomainEvilGlobal.Settings.AppOptions.MaxConnections)
                        .Select(x => new IPEndPoint(IPAddress.Any, p += 1)));
            _lastPort = p;
        }

        async Task TrySyncServers(IEnumerable<Server> objects, CancellationToken token,
            Action<ServerQueryState> callback) {
            try {
                await
                    objects.StartConcurrentTaskQueue(token, async server => {
                        var state = await SyncServer(server).ConfigureAwait(false);
                        if (callback != null && state != null)
                            callback(state);
                    },
                        () => DomainEvilGlobal.Settings.AppOptions.MaxConnections).ConfigureAwait(false);
            } finally {
                State.Progress = State.Maximum;
            }
        }

        async Task<ServerQueryState> SyncServer(Server server) {
            IPEndPoint endPoint = null;
            while (true) {
                var portInUse = false;
                try {
                    if (endPoint == null)
                        endPoint = _useRangedEndPoints ? GetEndPointFromQueue() : null;
                    var state = await server.UpdateAsync(endPoint).ConfigureAwait(false);
                    if (state.Canceled)
                        State.IncrementCancelled();
                    return state;
                } catch (SocketException e) {
                    if (e.SocketErrorCode == SocketError.AddressAlreadyInUse) {
                        this.Logger()
                            .FormattedWarnException(e, "Port in use: " + (endPoint == null ? -1 : endPoint.Port));
                        // replace used bad port with new port and retry, repeat until running out of ports
                        portInUse = true;
                        endPoint = GetNextEndPoint();
                    } else {
                        this.Logger().FormattedWarnException(e);
                        return null;
                    }
                } catch (OperationCanceledException e) {
#if DEBUG
                    this.Logger().FormattedDebugException(e);
#endif
                    return null;
                } catch (Exception e) {
                    this.Logger().FormattedWarnException(e);
                    return null;
                } finally {
                    if (_useRangedEndPoints && endPoint != null && !portInUse)
                        ReleaseEndPoint(endPoint);
                }
            }
        }

        IPEndPoint GetNextEndPoint() {
            var port = Interlocked.Increment(ref _lastPort);
            if (port > IPEndPoint.MaxPort)
                throw new RanOutOfPortsException("Reached MaxPort");
            return new IPEndPoint(IPAddress.Any, port);
        }

        IPEndPoint GetEndPointFromQueue() {
            IPEndPoint ep;
            var retry = 0;
            while (!_endpoints.TryDequeue(out ep)) {
                if (retry++ > 10) {
                    ep = null;
                    break;
                }
                Thread.Sleep(150);
            }
            if (ep == null)
                throw new RanOutOfPortsException("Endpoint not available from Queue");
            return ep;
        }

        void ReleaseEndPoint(IPEndPoint endPoint) {
            _endpoints.Enqueue(endPoint);
        }
    }

    class RanOutOfPortsException : Exception
    {
        public RanOutOfPortsException(string message) : base(message) {}
    }
}