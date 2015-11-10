// <copyright company="SIX Networks GmbH" file="InstallUserconfigCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;
using ShortBus;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands
{
    public class InstallUserconfigCommand : IAsyncRequest<UnitType>
    {
        public InstallUserconfigCommand(IAbsoluteDirectoryPath modPath, IAbsoluteDirectoryPath gameDirectory) {
            Contract.Requires<ArgumentNullException>(modPath != null);
            Contract.Requires<ArgumentNullException>(gameDirectory != null);

            ModPath = modPath;
            GameDirectory = gameDirectory;
        }

        public IAbsoluteDirectoryPath ModPath { get; private set; }
        public IAbsoluteDirectoryPath GameDirectory { get; private set; }
    }
}