// <copyright company="SIX Networks GmbH" file="ConnectionStateChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Connect.Infrastructure
{
    public class ConnectionStateChanged
    {
        public ConnectionStateChanged(bool isConnected) {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; private set; }
        public ConnectedState ConnectedState { get; set; }
    }
}