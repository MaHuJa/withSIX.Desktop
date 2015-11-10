// <copyright company="SIX Networks GmbH" file="UnpackCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Updater.Presentation.Wpf.Commands
{
    public class UnpackCommand : BaseCommand
    {
        bool _fullPath;
        bool _overwrite;

        public UnpackCommand() {
            IsCommand(UpdaterCommands.Unpack);
            HasOption("overwrite", "", a => _overwrite = a != null);
            HasOption("fullPath", "", a => _fullPath = a != null);
            HasAdditionalArguments(2, "<source> <output>");
        }

        public override int Run(string[] remainingArguments) {
            Tools.Compression.Unpack(remainingArguments[0].ToAbsoluteFilePath(),
                remainingArguments[1].ToAbsoluteDirectoryPath(), _overwrite, _fullPath);
            return 0;
        }
    }
}