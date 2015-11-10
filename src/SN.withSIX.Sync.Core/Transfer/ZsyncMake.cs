// <copyright company="SIX Networks GmbH" file="ZsyncMake.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IZsyncMake
    {
        void CreateZsyncFile(IAbsoluteFilePath fileName, ZsyncMakeOptions options = ZsyncMakeOptions.Default);

        void CreateZsyncFiles(IAbsoluteDirectoryPath rootDirectory, ZsyncMakeOptions options = ZsyncMakeOptions.Default,
            params string[] excludes);
    }

    [Flags]
    public enum ZsyncMakeOptions
    {
        Default = 0,
        Smart = 1,
        Thorough = 2,
        Overwrite = 4
    }

    public class ZsyncMake : IZsyncMake
    {
        static readonly string[] thoroughFiles = {"bundles.json", "packages.json", "objects.json", "config.json"};
        readonly Tools.FileTools.IFileOps _fileOps;
        readonly IProcessManager _processManager;

        public ZsyncMake(IProcessManager processManager, Tools.FileTools.IFileOps fileOps) {
            _processManager = processManager;
            _fileOps = fileOps;
        }

        /*
            MTime: Sun, 21 Apr 2013 14:18:24 +0000
            Length: 1087
            SHA-1: f7131dabff4bb0cf8057f5701c3eee51c8fbf779
         */

        public void CreateZsyncFile(IAbsoluteFilePath fileName, ZsyncMakeOptions options = ZsyncMakeOptions.Default) {
            var fileInfo = fileName.FileInfo;
            if (!options.HasFlag(ZsyncMakeOptions.Overwrite)) {
                var zsFile = (fileInfo.FullName + ".zsync").ToAbsoluteFilePath();
                if (zsFile.Exists) {
                    var thorough = options.HasFlag(ZsyncMakeOptions.Thorough) ||
                                   (options.HasFlag(ZsyncMakeOptions.Smart) && thoroughFiles.Contains(fileName.FileName));
                    if (!thorough)
                        return;

                    var data = _fileOps.ReadTextFile(zsFile);
                    var rxMtime = new Regex(@"MTime: (.*)");
                    var rxLength = new Regex(@"Length: (.*)");
                    var rxSha1 = new Regex(@"SHA-1: (.*)");
                    var match = rxMtime.Match(data);
                    if (match.Success) {
                        var mtime = DateTime.Parse(match.Groups[1].Value).ToUniversalTime();
                        var length = int.Parse(rxLength.Match(data).Groups[1].Value);
                        var sha1 = rxSha1.Match(data).Groups[1].Value;

                        // Mtime lacks ticks
                        var lastWrite = new DateTime(fileInfo.LastWriteTimeUtc.Year, fileInfo.LastWriteTimeUtc.Month,
                            fileInfo.LastWriteTimeUtc.Day, fileInfo.LastWriteTimeUtc.Hour,
                            fileInfo.LastWriteTimeUtc.Minute,
                            fileInfo.LastWriteTimeUtc.Second);

                        // sha1 check will probably make it as slow again as just recreating the zsync file, so we use mtime and length instead
                        if (lastWrite.Equals(mtime)
                            && fileInfo.Length == length)
                            return;
                    }
                }
            }
            var b128 = fileInfo.Length > FileSizeUnits.MB ? String.Empty : "-b 128 ";
            var parameters = String.Format("{1}-Z -u \"{0}\" -f \"{0}\" \"{0}\"", fileInfo.Name, b128);
            var startInfo =
                new ProcessStartInfo(Path.Combine(Common.Paths.ToolCygwinBinPath.ToString(), "zsyncmake.exe"),
                    parameters)
                    .SetWorkingDirectoryOrDefault(fileInfo.Directory.FullName);
            var info = _processManager.LaunchAndGrab(new BasicLaunchInfo(startInfo));
            var output = info.StandardOutput + "\n" + info.StandardError;
            if (info.ExitCode != 0) {
                throw new ZsyncMakeException(String.Format("ZsyncMake error: {0}", info.ExitCode), output,
                    parameters);
            }
        }

        public void CreateZsyncFiles(IAbsoluteDirectoryPath rootDirectory,
            ZsyncMakeOptions options = ZsyncMakeOptions.Default, params string[] excludes) {
            foreach (var file in Directory.EnumerateFiles(rootDirectory.ToString(), "*.*", SearchOption.AllDirectories)
                .Where(
                    x => !x.EndsWith(".zsync", StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.ToAbsoluteFilePath())
                .Where(x => !excludes.Contains(x.FileName))
                )
                CreateZsyncFile(file, options);
        }

        [DoNotObfuscate]
        public class ZsyncMakeException : Exception
        {
            public ZsyncMakeException(string message, string output = null, string parameters = null)
                : base(message) {
                Output = output;
                Parameters = parameters;
            }

            public string Output { get; private set; }
            public string Parameters { get; private set; }
        }
    }
}