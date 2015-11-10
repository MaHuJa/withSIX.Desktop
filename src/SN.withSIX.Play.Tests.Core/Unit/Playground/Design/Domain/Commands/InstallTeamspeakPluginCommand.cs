// <copyright company="SIX Networks GmbH" file="InstallTeamspeakPluginCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;
using ShortBus;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public class InstallTeamspeakPluginCommand : IAsyncRequest<UnitType>
    {
        public InstallTeamspeakPluginCommand(IAbsoluteDirectoryPath directoryInfo) {
            Contract.Requires<ArgumentNullException>(directoryInfo != null);
            DirectoryInfo = directoryInfo;
        }

        public IAbsoluteDirectoryPath DirectoryInfo { get; private set; }
    }
}