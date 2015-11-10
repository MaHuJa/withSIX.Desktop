// <copyright company="SIX Networks GmbH" file="UninstallCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Configuration.Install;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class UninstallCommand : BaseCommand
    {
        public UninstallCommand() {
            IsCommand(UpdaterCommands.UninstallService);
        }

        public override int Run(string[] remainingArguments) {
            if (!SixElevatedServiceMisc.IsElevatedServiceInstalled())
                return 0;

            ManagedInstallerClass.InstallHelper(new[] {"/u", Common.Paths.EntryLocation.ToString()});
            return 0;
        }
    }
}