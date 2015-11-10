// <copyright company="SIX Networks GmbH" file="PushCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class PushCommand : BaseCommand
    {
        readonly IRsyncLauncher _rsyncLauncher;
        readonly IZsyncMake _zsyncMake;
        public string Key;
        public bool MakeZsync;
        public bool Overwrite;

        public PushCommand(IRsyncLauncher launcher, IZsyncMake zsyncMake) {
            _rsyncLauncher = launcher;
            _zsyncMake = zsyncMake;

            IsCommand("push", "Rsync Push");
            HasAdditionalArguments(2, "<source> <destination>");
            HasOption("k|key=", "Path to key", s => Key = s);
            HasOption("m|makezsync", "Make zsync files", s => MakeZsync = s != null);
            HasOption("o|overwrite", "Overwrite zsync files (recreate even if matches)", s => Overwrite = s != null);
        }

        public override int Run(params string[] remainingArguments) {
            var src = remainingArguments[0];
            var dst = remainingArguments[1];
            if (MakeZsync) {
                System.Console.WriteLine("Creating zsync files...");
                _zsyncMake.CreateZsyncFiles(src.ToAbsoluteDirectoryPath(), GetOptions());
            }

            System.Console.WriteLine("Pushing...");

            var rm = new RsyncController(src, dst, Key, _rsyncLauncher);
            var status = new TransferStatus(src);
            using (
                new TimerWithoutOverlap(200,
                    () =>
                        System.Console.Write("\r" + status.Progress + "% " + Tools.FileUtil.GetFileSize(status.Speed) +
                                             "/s                    ")))
                rm.Push(status);

            System.Console.WriteLine("\nCompleted");

            return 0;
        }

        ZsyncMakeOptions GetOptions() {
            var options = ZsyncMakeOptions.Default;
            options &= ZsyncMakeOptions.Smart;
            if (Overwrite)
                options &= ZsyncMakeOptions.Overwrite;
            return options;
        }
    }
}