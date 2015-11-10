// <copyright company="SIX Networks GmbH" file="AbortedException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Legacy
{
    [DoNotObfuscate]
    public class AbortedException : Exception
    {
        public AbortedException() {}
        public AbortedException(string message) : base(message) {}
    }
}