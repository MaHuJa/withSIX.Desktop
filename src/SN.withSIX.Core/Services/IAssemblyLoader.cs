// <copyright company="SIX Networks GmbH" file="IAssemblyLoader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Services
{
    public interface IAssemblyInfo
    {
        string GetProductVersion();
        Version GetEntryVersion();
        string GetEntryAssemblyName();
        string GetEntryPath();
        string GetEntryLocation();
        string GetInformationalVersion();
    }

    public interface IAssemblyLoader : IAssemblyInfo {}
}