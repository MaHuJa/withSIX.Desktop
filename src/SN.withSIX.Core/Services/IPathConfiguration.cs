// <copyright company="SIX Networks GmbH" file="IPathConfiguration.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;

namespace SN.withSIX.Core.Services
{
    public interface IPathConfiguration
    {
        IAbsoluteDirectoryPath AppPath { get; }
        IAbsoluteDirectoryPath ConfigPath { get; }
        IAbsoluteDirectoryPath DataPath { get; }
        IAbsoluteDirectoryPath LocalDataPath { get; }
        IAbsoluteDirectoryPath LogPath { get; }
        IAbsoluteDirectoryPath MyDocumentsPath { get; }
        IAbsoluteDirectoryPath TempPath { get; }
        IAbsoluteDirectoryPath ToolMinGwBinPath { get; }
        IAbsoluteDirectoryPath ToolCygwinBinPath { get; }
        IAbsoluteDirectoryPath ToolPath { get; }
        bool PathsSet { get; }
        IAbsoluteDirectoryPath StartPath { get; }
        IAbsoluteDirectoryPath JavaPath { get; }
        IAbsoluteDirectoryPath SteamPath { get; }
    }
}