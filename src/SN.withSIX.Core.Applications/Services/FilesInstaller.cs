// <copyright company="SIX Networks GmbH" file="FilesInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Core.Applications.Services
{
    public class DownloadAndInstaller
    {
        readonly IAbsoluteDirectoryPath _destination;
        readonly IFileDownloader _downloader;
        readonly string _file;
        readonly IStatus _progress;
        readonly IRestarter _restarter;
        readonly StatusRepo _statusRepo;
        IAbsoluteFilePath _sourceFile;

        public DownloadAndInstaller(IFileDownloader downloader, StatusRepo statusRepo, string file,
            IAbsoluteDirectoryPath destination, IRestarter restarter) {
            _downloader = downloader;
            _statusRepo = statusRepo;
            _file = file;
            _destination = destination;
            _restarter = restarter;
            _sourceFile = Common.Paths.TempPath.GetChildFileWithName(_file);
            _progress = new Status(_file, statusRepo, 0, 0, default(TimeSpan?), RepoStatus.Downloading);
        }

        public async Task DownloadAndInstall() {
            if (await PreConditions().ConfigureAwait(false))
                return;

            var sourceFile = Common.Paths.AppPath.GetChildFileWithName(_file);
            if (sourceFile.Exists) {
                _sourceFile = sourceFile;
                await PerformUnpack().ConfigureAwait(false);
                return;
            }

            await PerformDownload().ConfigureAwait(false);
            await PerformUnpack().ConfigureAwait(false);
        }

        async Task<bool> PreConditions() {
            if (await _restarter.CheckUac(_destination).ConfigureAwait(false))
                return true;

            _statusRepo.UpdateProgress(15);
            return false;
        }

        async Task PerformDownload() {
            _progress.Reset(RepoStatus.Downloading);
            DeleteSourceFileIfExists();
            Directory.CreateDirectory(_sourceFile.ParentDirectoryPath.ToString());
            await
                _downloader.DownloadAsync(Tools.Transfer.JoinUri(CommonUrls.SoftwareUpdateUri, _file), _sourceFile,
                    _progress).ConfigureAwait(false);
            _statusRepo.UpdateProgress(50);
        }

        async Task PerformUnpack() {
            _statusRepo.Action = RepoStatus.Unpacking;
            _progress.Reset(RepoStatus.Unpacking);
            await _restarter.TryWithUacFallback(Task.Factory.StartNew(
                () => Tools.Compression.Unpack(_sourceFile, _destination, true), TaskCreationOptions.LongRunning),
                "files").ConfigureAwait(false);
            _progress.Progress = 100;
            _statusRepo.UpdateProgress(90);
        }

        void DeleteSourceFileIfExists() {
            if (_sourceFile.Exists)
                Tools.FileUtil.Ops.DeleteWithRetry(_sourceFile.ToString());
        }
    }

    public abstract class FilesInstaller
    {
        readonly IFileDownloader _downloader;
        readonly IRestarter _restarter;

        protected FilesInstaller(IFileDownloader downloader, IRestarter restarter) {
            _downloader = downloader;
            _restarter = restarter;
        }

        protected Task DownloadAndInstall(StatusRepo statusRepo, string file, IAbsoluteDirectoryPath destination) {
            return new DownloadAndInstaller(_downloader, statusRepo, file, destination, _restarter).DownloadAndInstall();
        }
    }
}