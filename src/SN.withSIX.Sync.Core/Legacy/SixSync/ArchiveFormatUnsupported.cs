// <copyright company="SIX Networks GmbH" file="ArchiveFormatUnsupported.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    [DoNotObfuscate]
    public class ArchiveFormatUnsupported : Exception
    {
        public ArchiveFormatUnsupported() {}
        public ArchiveFormatUnsupported(string message) : base(message) {}
    }
}