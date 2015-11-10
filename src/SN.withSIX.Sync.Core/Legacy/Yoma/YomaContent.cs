// <copyright company="SIX Networks GmbH" file="YomaContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy.Yoma
{
    [DoNotObfuscate]
    class InvalidFileHash : Exception
    {
        public InvalidFileHash(string message) : base(message) {}
    }

    public class YomaContent
    {
        static readonly string ArchiveExtension = ".archive";
        readonly IFileDownloadHelper _downloader;
        readonly string[] YomaConfigFiles = {"Addons.xml", "Mods.xml", "Server.xml"};

        public YomaContent(IAbsoluteDirectoryPath destination, Uri uri, IFileDownloadHelper downloader) {
            Contract.Requires<ArgumentNullException>(destination != null);
            _downloader = downloader;
            Destination = destination;
            YasDir = Destination.GetChildDirectoryWithName(".yas");
            FilesDir = YasDir.GetChildDirectoryWithName("files");
            Url = uri;
            ConfigArchive = YasDir.GetChildFileWithName("config" + ArchiveExtension);
            TmpPath = YasDir.GetChildDirectoryWithName("tmp");
        }

        public Uri Url { get; }
        public IAbsoluteDirectoryPath Destination { get; }
        public IAbsoluteDirectoryPath YasDir { get; }
        public IAbsoluteDirectoryPath FilesDir { get; }
        public IAbsoluteFilePath ConfigArchive { get; }
        public YomaConfig Config { get; set; }
        public IAbsoluteDirectoryPath TmpPath { get; }

        public void Create() {
            YasDir.MakeSurePathExists();
        }

        public Task DownloadConfig(string config) {
            Contract.Requires<ArgumentNullException>(config != null);
            return _downloader.DownloadFileAsync(Tools.Transfer.JoinUri(Url, config), ConfigArchive);
        }

        public Task Download() {
            return DownloadMod(null);
        }

        bool ConfirmFileValidity(IAbsoluteFilePath file, string md5) {
            if (!file.Exists)
                return false;

            var fileMd5 = Tools.HashEncryption.MD5FileHash(file);

            return fileMd5 == md5;
        }

        public virtual void DownloadAddon(YomaConfig.YomaAddon addon) {
            var destination = FilesDir.GetChildFileWithName(addon.Url);
            var destinationUnpacked = Path.Combine(Destination.ToString(), addon.Path, addon.Pbo).ToAbsoluteFilePath();

            if (ConfirmFileValidity(destinationUnpacked, addon.Md5))
                return;

            var directory = destination.ParentDirectoryPath.ToString();
            directory.MakeSurePathExists();
            _downloader.DownloadFileAsync(Tools.Transfer.JoinUri(Url, addon.Url), destination);
        }

        public virtual async Task DownloadMod(YomaConfig.YomaMod mod) {
            Config.Addons.Where(x => !File.Exists(Path.Combine(Destination.ToString(), x.Path, x.Pbo))
                                     && FilesDir.GetChildFileWithName(x.Url).Exists).ForEach(UnpackAddon);
            var remoteFiles = Config.Addons
                .Where(
                    x =>
                        !ConfirmFileValidity(Path.Combine(Destination.ToString(), x.Path, x.Pbo).ToAbsoluteFilePath(),
                            x.Md5))
                .Select(x => x.Url);

            using (var statusRepo = new StatusRepo()) {
                await _downloader.DownloadFilesAsync(new[] {Url}, statusRepo,
                    remoteFiles.ToDictionary(x => new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(x, null),
                        x => (ITransferStatus) null), FilesDir).ConfigureAwait(false);
            }
        }

        public virtual void UnpackAddon(YomaConfig.YomaAddon addon) {
            var destination = FilesDir.GetChildFileWithName(addon.Url);
            var destinationUnpacked = Path.Combine(Destination.ToString(), addon.Path, addon.Pbo).ToAbsoluteFilePath();

            var directory = destinationUnpacked.ParentDirectoryPath;
            directory.MakeSurePathExists();

            Tools.Compression.Unpack(destination, directory, true);

            if (!ConfirmFileValidity(destinationUnpacked, addon.Md5)) {
                Tools.FileUtil.Ops.DeleteWithRetry(destination.ToString());
                Tools.FileUtil.Ops.DeleteWithRetry(destinationUnpacked.ToString());
                throw new InvalidFileHash(String.Format("{0} should be {1}", destinationUnpacked, addon.Md5));
            }
        }

        public virtual void UnpackConfig() {
            Tools.Compression.Unpack(ConfigArchive, TmpPath, true, false);
        }

        public virtual void UnpackMod() {
            Config.Addons
                .Where(x => FilesDir.GetChildFileWithName(x.Url).Exists
                            &&
                            !ConfirmFileValidity(
                                Path.Combine(Destination.ToString(), x.Path, x.Pbo).ToAbsoluteFilePath(), x.Md5))
                .ForEach(UnpackAddon);
        }

        public virtual void ParseConfig() {
            Config = new YomaConfig(TmpPath.GetChildFileWithName(YomaConfigFiles[0]),
                TmpPath.GetChildFileWithName(YomaConfigFiles[1]),
                TmpPath.GetChildFileWithName(YomaConfigFiles[2]));
        }
    }
}