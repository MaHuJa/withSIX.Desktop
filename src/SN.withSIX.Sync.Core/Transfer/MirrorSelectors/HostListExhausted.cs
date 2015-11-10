// <copyright company="SIX Networks GmbH" file="HostListExhausted.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Transfer.MirrorSelectors
{
    [DoNotObfuscate]
    public class HostListExhausted : TransferException
    {
        public HostListExhausted() {}
        public HostListExhausted(string message) : base(message) {}
        public HostListExhausted(string message, Exception inner) : base(message, inner) {}
    }
}