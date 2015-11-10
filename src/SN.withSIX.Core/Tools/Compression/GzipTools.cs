// <copyright company="SIX Networks GmbH" file="GzipTools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NDepend.Path;
using SharpCompress.Archive;
using SharpCompress.Archive.GZip;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public class GzipTools
        {
            public virtual string Gzip(IAbsoluteFilePath file, IAbsoluteFilePath dest = null,
                bool preserveFileNameAndModificationTime = false) {
                Contract.Requires<ArgumentNullException>(file != null);
                Contract.Requires<ArgumentException>(file.Exists);

                var defDest = (file + ".gz").ToAbsoluteFilePath();
                if (dest == null)
                    dest = defDest;

                var cmd = String.Format("-f --best --rsyncable --keep \"{0}\"", file);
                if (!preserveFileNameAndModificationTime)
                    cmd = "-n " + cmd;

                dest.RemoveReadonlyWhenExists();

                var startInfo =
                    new ProcessStartInfoBuilder(Common.Paths.ToolPath.GetChildFileWithName("gzip.exe"), cmd) {
                        WorkingDirectory = file.ParentDirectoryPath.ToString()
                    }.Build();
                var ret = ProcessManager.LaunchAndGrabTool(startInfo, "Gzip pack");

                if (Path.GetFullPath(dest.ToString()) != Path.GetFullPath(defDest.ToString()))
                    FileUtil.Ops.MoveWithRetry(defDest, dest);

                return ret.StandardOutput + ret.StandardError;
            }

            public virtual string GzipStdOut(IAbsoluteFilePath inputFile, IAbsoluteFilePath outputFile = null,
                bool preserveFileNameAndModificationTime = false) {
                Contract.Requires<ArgumentException>(inputFile != null);
                Contract.Requires<ArgumentException>(inputFile.Exists);

                if (outputFile == null)
                    outputFile = (inputFile + ".gz").ToAbsoluteFilePath();

                var cmd = String.Format("-f --best --rsyncable --keep --stdout \"{0}\" > \"{1}\"",
                    inputFile, outputFile);
                if (!preserveFileNameAndModificationTime)
                    cmd = "-n " + cmd;

                outputFile.RemoveReadonlyWhenExists();
                var startInfo =
                    new ProcessStartInfoBuilder(Common.Paths.ToolPath.GetChildFileWithName("gzip.exe"), cmd) {
                        WorkingDirectory = Common.Paths.LocalDataPath.ToString()
                    }.Build();
                var ret = ProcessManager.LaunchAndGrabToolCmd(startInfo, "Gzip pack");
                return ret.StandardOutput + ret.StandardError;
            }

            public virtual string GzipAuto(IAbsoluteFilePath inputFile, IAbsoluteFilePath outputFile = null,
                bool preserveFileNameAndModificationTime = false) {
                if (inputFile.ToString().EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase))
                    return GzipStdOut(inputFile, outputFile, preserveFileNameAndModificationTime);
                return Gzip(inputFile, outputFile, preserveFileNameAndModificationTime);
            }

            public virtual byte[] DecompressGzip(byte[] compressedBytes) {
                Contract.Requires<ArgumentNullException>(compressedBytes != null);

                using (var ms = new MemoryStream(compressedBytes)) {
                    var step = new byte[16]; //Instead of 16 can put any 2^x
                    using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                    using (var outStream = new MemoryStream()) {
                        int readCount;
                        do {
                            readCount = zip.Read(step, 0, step.Length);
                            outStream.Write(step, 0, readCount);
                        } while (readCount > 0);
                        return outStream.ToArray();
                    }
                }
            }

            public void UnpackSingleGzip(IAbsoluteFilePath sourceFile, IAbsoluteFilePath destFile,
                ITProgress progress = null) {
                using (var archive = GZipArchive.Open(sourceFile.ToString())) {
                    if (progress != null) {
                        archive.CompressedBytesRead += (sender, args) => {
                            double prog = (args.CompressedBytesRead/(float) archive.TotalSize);
                            if (prog > 1)
                                prog = 1;
                            progress.Progress = prog*100;
                        };
                    }
                    destFile.RemoveReadonlyWhenExists();
                    var entry = archive.Entries.First();

                    entry.WriteToFile(destFile.ToString());
                }
            }

            public void UnpackSingleGzipWithFallbackAndRetry(IAbsoluteFilePath sourceFile, IAbsoluteFilePath destFile) {
                FileUtil.Ops.AddIORetryDialog(() => {
                    try {
                        UnpackSingleGzip(sourceFile, destFile);
                    } catch (UnauthorizedAccessException) {
                        if (!Processes.Uac.CheckUac())
                            throw;
                        UnpackSingleZipWithUpdaters(sourceFile, destFile);
                    }
                });
            }

            static void UnpackSingleZipWithUpdaters(IAbsoluteFilePath sourceFile, IAbsoluteFilePath destFile) {
                Generic.RunUpdater(UpdaterCommands.UnpackSingleGzip, sourceFile.ToString(), destFile.ToString());
            }
        }
    }
}