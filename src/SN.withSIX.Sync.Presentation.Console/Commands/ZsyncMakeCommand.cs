// <copyright company="SIX Networks GmbH" file="ZsyncMakeCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Linq;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class ZsyncMakeCommand : BaseCommand
    {
        readonly IZsyncMake _zsyncMake;
        public bool Overwrite;
        public bool Smart;
        public bool Thorough;

        public ZsyncMakeCommand(IZsyncMake zsyncMake) {
            _zsyncMake = zsyncMake;
            IsCommand("zsyncmake", "Zsync Make handling");
            HasOption("o|overwrite", "Overwrite zsync files (recreate even if matches)", s => Overwrite = s != null);
            HasOption("t|thorough", "Check zsync files to see if they need to be re-generated",
                s => Thorough = s != null);
            HasOption("s|smart", "Use thorough only where applicable", s => Smart = s != null);
            AllowsAnyAdditionalArguments("<file or folder> (<file or folder>...)");
        }

        public override int Run(params string[] remainingArguments) {
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one file or folder");

            var options = GetOptions();
            foreach (var p in remainingArguments) {
                if (Directory.Exists(p)) {
                    System.Console.WriteLine("Processing folder: {0}", p);
                    _zsyncMake.CreateZsyncFiles(p.ToAbsoluteDirectoryPath(), options, Repository.LockFile,
                        Repository.SerialFile);
                } else {
                    System.Console.WriteLine("Processing file: {0}", p);
                    _zsyncMake.CreateZsyncFile(p.ToAbsoluteFilePath(), options);
                }
            }

            return 0;
        }

        ZsyncMakeOptions GetOptions() {
            var options = ZsyncMakeOptions.Default;
            if (Overwrite)
                options = options | ZsyncMakeOptions.Overwrite;
            if (Thorough)
                options = options | ZsyncMakeOptions.Thorough;
            if (Smart)
                options = options | ZsyncMakeOptions.Smart;
            return options;
        }
    }
}