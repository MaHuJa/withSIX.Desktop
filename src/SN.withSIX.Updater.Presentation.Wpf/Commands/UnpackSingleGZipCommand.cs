// <copyright company="SIX Networks GmbH" file="UnpackSingleGZipCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class UnpackSingleGZipCommand : BaseCommand
    {
        public UnpackSingleGZipCommand() {
            IsCommand(UpdaterCommands.UnpackSingleGzip);
            HasAdditionalArguments(2, "<source> <output>");
        }

        public override int Run(string[] remainingArguments) {
            Tools.Compression.Gzip.UnpackSingleGzip(remainingArguments[0].ToAbsoluteFilePath(),
                remainingArguments[1].ToAbsoluteFilePath());
            return 0;
        }
    }
}