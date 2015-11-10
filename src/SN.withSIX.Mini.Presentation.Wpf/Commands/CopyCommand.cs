// <copyright company="SIX Networks GmbH" file="CopyCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Presentation.Wpf.Commands
{
    public class CopyCommand : BaseCommand
    {
        bool _overwrite;

        public CopyCommand() {
            IsCommand(UpdaterCommands.Copy);
            HasOption("overwrite", "Overwrite the destination file", a => _overwrite = a != null);
            HasAdditionalArguments(2, "<source> <destination>");
        }

        public override int Run(string[] remainingArguments) {
            Tools.FileUtil.Ops.CopyWithRetry(remainingArguments[0].ToAbsoluteFilePath(),
                remainingArguments[1].ToAbsoluteFilePath(), _overwrite);
            return 0;
        }
    }
}