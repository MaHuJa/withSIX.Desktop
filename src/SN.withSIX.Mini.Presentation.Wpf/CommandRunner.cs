// <copyright company="SIX Networks GmbH" file="CommandRunner.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Mini.Presentation.Wpf.Commands;

namespace SN.withSIX.Mini.Presentation.Wpf
{
    public class CommandRunner : IPresentationService
    {
        readonly IOrderedEnumerable<BaseCommand> _commands;

        public CommandRunner(IEnumerable<BaseCommand> commands) {
            _commands = commands.OrderBy(x => x.GetType().Name);
        }

        public int RunCommandsAndLog(string[] args) {
            var r = RunCommands(args);
            if (r != 0)
                MainLog.Logger.Error("Error {0} dispatching command.", r);
            return r;
        }

        int RunCommands(string[] args) {
            return ConsoleCommandDispatcher.DispatchCommand(_commands, args, Console.Out);
        }
    }
}