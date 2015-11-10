// <copyright company="SIX Networks GmbH" file="ServerQueryState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class ServerQueryState : IEnableLogging
    {
        readonly object _closedLock = new Object();
        readonly Stopwatch _stopWatch = new Stopwatch();

        public ServerQueryState(IPEndPoint ep = null) {
            MaxPackets = 7;
            EndPoint = ep ?? new IPEndPoint(IPAddress.Any, 0);
            ReceivedPackets = new Dictionary<int, IEnumerable<byte>>();
            Pings = new List<long>();
        }

        public IPEndPoint EndPoint { get; private set; }
        public UdpClient Client { get; set; }
        public Exception Exception { get; set; }
        public List<long> Pings { get; }
        public int MaxPackets { get; set; }
        public Dictionary<int, IEnumerable<byte>> ReceivedPackets { get; private set; }
        public ServerQueryResult Result { get; private set; }
        public Server Server { get; set; }
        public bool Canceled { get; private set; }
        public bool Exited { get; private set; }
        public bool Success { get; private set; }
        public bool Closed { get; private set; }

        public void ProcessParser(IServerQueryParser parser) {
            if (Pings.Count == 0)
                Pings.Add(Common.MagicPingValue);
            Result = parser.ParsePackets(this);
            ReceivedPackets = null;
            Succeed();
        }

        public virtual void Cancel() {
            ReceivedPackets = null;
            TryClose();
            if (Server != null)
                Server.Ping = Common.MagicPingValue;
            Canceled = true;
        }

        public virtual void Exit(bool close = true) {
            TryClose();
            if (Server != null)
                Server.Ping = Common.MagicPingValue;
            Exited = true;
        }

        protected virtual void Succeed(bool close = true) {
            TryClose();
            Success = true;
            Exited = true;
        }

        public virtual void StartSW() {
            _stopWatch.Start();
        }

        public virtual void StopSW() {
            _stopWatch.Stop();
            if (_stopWatch.ElapsedMilliseconds > 0)
                Pings.Add(_stopWatch.ElapsedMilliseconds);
            _stopWatch.Reset();
        }

        public virtual bool IsRunningSW() {
            return _stopWatch.IsRunning;
        }

        protected virtual bool TryClose() {
            if (Client == null)
                return true;
            lock (_closedLock) {
                if (Closed)
                    return true;
                Closed = true;
            }

            try {
                Client.Close();
                return true;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }

            return false;
        }
    }
}