// <copyright company="SIX Networks GmbH" file="InstalledState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public class InstalledState
    {
        public InstalledState(IAbsoluteFilePath executable, IAbsoluteFilePath launchExecutable,
            IAbsoluteDirectoryPath directory, IAbsoluteDirectoryPath workingDirectory, Version version = null,
            bool isClient = true) {
            Contract.Requires<ArgumentNullException>(executable != null);
            Contract.Requires<ArgumentNullException>(launchExecutable != null);
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<ArgumentNullException>(workingDirectory != null);

            Executable = executable;
            LaunchExecutable = launchExecutable;
            Directory = directory;
            WorkingDirectory = workingDirectory;
            Version = version;
            IsClient = isClient;
        }

        protected InstalledState() {}
        public virtual bool IsInstalled
        {
            get { return true; }
        }
        public bool IsClient { get; private set; }
        public IAbsoluteDirectoryPath WorkingDirectory { get; private set; }
        public IAbsoluteDirectoryPath Directory { get; private set; }
        public Version Version { get; private set; }
        public IAbsoluteFilePath Executable { get; private set; }
        public IAbsoluteFilePath LaunchExecutable { get; private set; }
    }

    public class NotInstalledState : InstalledState
    {
        public override bool IsInstalled
        {
            get { return false; }
        }
    }
}