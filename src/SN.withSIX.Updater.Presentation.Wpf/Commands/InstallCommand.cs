// <copyright company="SIX Networks GmbH" file="InstallCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Configuration.Install;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class InstallCommand : BaseCommand
    {
        public InstallCommand() {
            IsCommand(UpdaterCommands.InstallService);
        }

        public override int Run(string[] remainingArguments) {
            if (SixElevatedServiceMisc.IsElevatedServiceInstalled())
                return 0;

            ManagedInstallerClass.InstallHelper(new[] {Common.Paths.EntryLocation.ToString()});
            return 0;
        }
    }
}