// <copyright company="SIX Networks GmbH" file="ConnectedState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Connect
{
    [DoNotObfuscateType]
    public enum ConnectedState
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectingFailed
    }
}