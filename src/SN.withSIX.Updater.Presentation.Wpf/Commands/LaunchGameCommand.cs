// <copyright company="SIX Networks GmbH" file="LaunchGameCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Wpf.Services;
using SN.withSIX.Updater.Presentation.Wpf.Services;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class LaunchGameCommand : BaseCommand
    {
        readonly GameLauncher.GameLaunchSpec _spec = new GameLauncher.GameLaunchSpec {
            Priority = ProcessPriorityClass.Normal
        };
        public int ProcessID;

        public LaunchGameCommand() {
            IsCommand(UpdaterCommands.LaunchGame, "Launch a selected game.");
            HasRequiredOption("gamePath=", "Path the the Game Executable", a => _spec.GamePath = a);
            HasOption("workingDirectory=", "The directory you wish to launch Game in.",
                a => _spec.WorkingDirectory = a);
            HasOption("steamPath=", "Path to Steam", a => _spec.SteamPath = a);
            HasOption("steamID=", "The Steam App ID of the Game being launched", a => _spec.SteamID = a.TryInt());
            HasOption("bypassUAC", "Bypass User Account Control", a => _spec.BypassUAC = a != null);
            HasOption("steamDRM", "Launch Game using the Steam DRM system. (Some games may require this)",
                a => _spec.SteamDRM = a != null);
            HasOption("legacyLaunch", "Launch over legacy steam", a => _spec.LegacyLaunch = a != null);
            HasOption("priority=", "Launch game with priority", a => {
                _spec.Priority = !string.IsNullOrWhiteSpace(a)
                    ? (ProcessPriorityClass) Enum.Parse(typeof (ProcessPriorityClass), a)
                    : ProcessPriorityClass.Normal;
            });
            HasOption("affinity=", "Launch game with affinity",
                a =>
                    _spec.Affinity =
                        string.IsNullOrWhiteSpace(a) ? new int[0] : a.Split(',').Select(int.Parse).ToArray());
            HasOption("arguments=", "Arguments will be sent to Game", a => _spec.Arguments = a);
        }

        public override int Run(params string[] remainingArguments) {
            this.Logger().Info(_spec.Inspect().PrettyPrint());
            var shutdownHandler = new WpfShutdownHandler(new ExitHandler());
            ProcessID =
                new GameLauncher(shutdownHandler,
                    new Restarter(shutdownHandler, new WpfDialogManager(new WindowManager()))).LaunchGame(_spec);
            return 0;
        }
    }
}