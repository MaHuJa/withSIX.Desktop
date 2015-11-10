// <copyright company="SIX Networks GmbH" file="MoveCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class MoveCommand : BaseCommand
    {
        bool _overwrite;

        public MoveCommand() {
            IsCommand(UpdaterCommands.Move);
            HasOption("overwrite", "Overwrite the destination file", a => _overwrite = a != null);
            HasAdditionalArguments(2, "<source> <destination>");
        }

        public override int Run(string[] remainingArguments) {
            Tools.FileUtil.Ops.MoveWithRetry(remainingArguments[0].ToAbsoluteFilePath(),
                remainingArguments[1].ToAbsoluteFilePath());
            return 0;
        }
    }
}