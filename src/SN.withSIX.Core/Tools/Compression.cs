// <copyright company="SIX Networks GmbH" file="Compression.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SevenZip;
using SharpCompress.Archive;
using SharpCompress.Archive.GZip;
using SharpCompress.Common;
using SharpCompress.Writer;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core
{
    public static partial class Tools
    {
        public static CompressionTools Compression = new CompressionTools();

        #region Nested type: Compression

        public class CompressionTools
        {
            public static readonly string[] SevenzipArchiveFormats = {
                ".7z",
                ".archive",
                ".rar",
                ".zip",
                ".bzip2",
                ".xz",
                ".tar",
                ".cab",
                ".lzh",
                ".lzma"
            };
            public GzipTools Gzip = new GzipTools();

            public virtual void Unpack(IAbsoluteFilePath sourceFile, IAbsoluteDirectoryPath outputFolder,
                bool overwrite = false, bool fullPath = true, bool force7z = false, bool checkFileIntegrity = true,
                ITProgress progress = null) {
                Contract.Requires<ArgumentNullException>(sourceFile != null);
                Contract.Requires<ArgumentNullException>(outputFolder != null);

                var ext = sourceFile.FileExtension;
                if (force7z ||
                    SevenzipArchiveFormats.Any(x => ext.Equals(x, StringComparison.OrdinalIgnoreCase))) {
                    using (var extracter = new SevenZipExtractor(sourceFile.ToString()))
                        UnpackArchive(sourceFile, outputFolder, overwrite, checkFileIntegrity, extracter);
                } else {
                    var options = fullPath ? ExtractOptions.ExtractFullPath : ExtractOptions.None;
                    using (var archive = GetArchiveWithGzWorkaround(sourceFile, ext))
                        UnpackArchive(outputFolder, overwrite, archive, options);
                }
            }

            public void PackTar(IAbsoluteDirectoryPath directory, IAbsoluteFilePath outputFile) {
                using (var tarStream = File.OpenWrite(outputFile.ToString()))
                using (var af = WriterFactory.Open(tarStream, ArchiveType.Tar, CompressionType.None)) {
                    foreach (var f in directory.DirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                        af.Write(f.FullName.Replace(directory.ParentDirectoryPath + @"\", ""), f.FullName);
                    // This ommits the root folder ('userconfig')
                    //af.WriteAll(directory.ToString(), "*", SearchOption.AllDirectories);
                }
            }

            static void UnpackArchive(IAbsoluteFilePath sourceFile, IAbsoluteDirectoryPath outputFolder, bool overwrite,
                bool checkFileIntegrity,
                SevenZipExtractor extracter) {
                if (checkFileIntegrity && !extracter.Check())
                    throw new Exception(String.Format("Appears to be an invalid archive: {0}", sourceFile));
                outputFolder.MakeSurePathExists();
                extracter.ExtractFiles(outputFolder.ToString(), overwrite
                    ? extracter.ArchiveFileNames.ToArray()
                    : extracter.ArchiveFileNames.Where(x => !outputFolder.GetChildFileWithName(x).Exists)
                        .ToArray());
            }

            static IArchive GetArchiveWithGzWorkaround(IAbsoluteFilePath sourceFile, string ext) {
                return ext.Equals(".gz", StringComparison.OrdinalIgnoreCase)
                    ? GZipArchive.Open(sourceFile.ToString())
                    : ArchiveFactory.Open(sourceFile.ToString());
            }

            static void UnpackArchive(IAbsoluteDirectoryPath outputFolder, bool overwrite, IArchive archive,
                ExtractOptions options) {
                foreach (var p in archive.Entries.Where(entry => entry.IsDirectory)
                    .Select(entry => outputFolder.GetChildDirectoryWithName(entry.Key)))
                    p.MakeSurePathExists();

                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory)) {
                    if (overwrite)
                        options = options | ExtractOptions.Overwrite;

                    if (!overwrite) {
                        if (!outputFolder.GetChildFileWithName(entry.Key).Exists)
                            entry.WriteToDirectory(outputFolder.ToString(), options);
                    } else
                        entry.WriteToDirectory(outputFolder.ToString(), options);
                }
            }

            public void UnpackSingle(string sourceFile, string destFile) {
                using (var archive = ArchiveFactory.Open(sourceFile)) {
                    var entry = archive.Entries.First();
                    entry.WriteToFile(destFile);
                }
            }

            public virtual void UnpackRetryUpdater(IAbsoluteFilePath sourceFile, IAbsoluteDirectoryPath outputFolder,
                bool overwrite = false, bool fullPath = true) {
                Contract.Requires<ArgumentNullException>(sourceFile != null);
                Contract.Requires<ArgumentNullException>(outputFolder != null);

                FileUtil.Ops.AddIORetryDialog(() => {
                    try {
                        Unpack(sourceFile, outputFolder, overwrite, fullPath);
                    } catch (UnauthorizedAccessException) {
                        if (!Processes.Uac.CheckUac())
                            throw;
                        UnpackUpdater(sourceFile, outputFolder, overwrite, fullPath);
                    } catch (IOException) {
                        if (!Processes.Uac.CheckUac())
                            throw;
                        UnpackUpdater(sourceFile, outputFolder, overwrite, fullPath);
                    }
                });
            }

            public virtual void UnpackUpdater(IAbsoluteFilePath sourceFile, IAbsoluteDirectoryPath outputFolder,
                bool overwrite = false, bool fullPath = true) {
                Contract.Requires<ArgumentNullException>(sourceFile != null);
                Contract.Requires<ArgumentNullException>(outputFolder != null);

                Generic.RunUpdater(UpdaterCommands.Unpack, sourceFile.ToString(), outputFolder.ToString(),
                    overwrite ? "--overwrite" : null,
                    fullPath.ToString());
            }

            public virtual void PackSevenZipNative(IAbsoluteFilePath file, IAbsoluteFilePath dest = null) {
                Contract.Requires<ArgumentNullException>(file != null);
                Contract.Requires<ArgumentException>(file.Exists);

                if (dest == null)
                    dest = (file + ".7z").ToAbsoluteFilePath();

                var compressor = new SevenZipCompressor {
                    CompressionLevel = CompressionLevel.Ultra,
                    CompressionMethod = CompressionMethod.Lzma2,
                    CompressionMode = CompressionMode.Create
                };

                var dir = dest.ParentDirectoryPath;
                dir.MakeSurePathExists();

                compressor.CompressFiles(dest.ToString(), file.ToString());
            }
        }

        #endregion
    }
}