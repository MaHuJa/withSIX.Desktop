// <copyright company="SIX Networks GmbH" file="MoveDirectoryCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Presentation.Wpf.Commands
{
    public class MoveDirectoryCommand : BaseCommand
    {
        public MoveDirectoryCommand() {
            IsCommand(UpdaterCommands.MoveDirectory);
            HasAdditionalArguments(2, "<source> <destination>");
        }

        public override int Run(string[] remainingArguments) {
            Tools.FileUtil.Ops.MoveDirectory(remainingArguments[0].ToAbsoluteDirectoryPath(),
                remainingArguments[1].ToAbsoluteDirectoryPath());
            return 0;
        }
    }
}