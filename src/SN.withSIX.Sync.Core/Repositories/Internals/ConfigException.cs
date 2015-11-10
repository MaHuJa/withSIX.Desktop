// <copyright company="SIX Networks GmbH" file="ConfigException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    [DoNotObfuscate]
    public class ConfigException : Exception
    {
        public ConfigException() {}
        public ConfigException(string message) : base(message) {}
    }
}