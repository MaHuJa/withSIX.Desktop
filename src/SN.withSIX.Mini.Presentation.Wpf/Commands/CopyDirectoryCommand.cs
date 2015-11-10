// <copyright company="SIX Networks GmbH" file="CopyDirectoryCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Mini.Presentation.Wpf.Commands
{
    public class CopyDirectoryCommand : BaseCommand
    {
        bool _overwrite;

        public CopyDirectoryCommand() {
            IsCommand(UpdaterCommands.CopyDirectory);
            HasAdditionalArguments(2, "<source> <destination>");
            HasOption("overwrite", "", a => _overwrite = a != null);
        }

        public override int Run(string[] remainingArguments) {
            try {
                Tools.FileUtil.Ops.CopyDirectoryWithRetry(remainingArguments[0].ToAbsoluteDirectoryPath(),
                    remainingArguments[1].ToAbsoluteDirectoryPath(), _overwrite);
            } catch (IOException e) {
                if (_overwrite)
                    throw;
                MainLog.Logger.FormattedWarnException(e);
            }
            return 0;
        }
    }
}