// <copyright company="SIX Networks GmbH" file="FileDownloadHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Legacy
{
    public interface IFileDownloadHelper
    {
        Task DownloadFilesAsync(IReadOnlyCollection<Uri> remotes, StatusRepo sr,
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destination);

        Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            IReadOnlyCollection<Uri> remotes,
            CancellationToken token, int limit);

        Task DownloadFilesAsync(IReadOnlyCollection<Uri> remotes, StatusRepo sr,
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destination,
            int limit);

        Task DownloadFileAsync(Uri uri, IAbsoluteFilePath localFile);

        Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            IReadOnlyCollection<Uri> remotes, CancellationToken token, int limit,
            Func<IAbsoluteFilePath, bool> confirmValidity);
    }

    public class FileDownloadHelper : IFileDownloadHelper
    {
        readonly Func<IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>> _createMirrorSelector;
        readonly Func<int, IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>>
            _createMirrorSelectorWithLimit;
        readonly Func<IMirrorSelector, ExportLifetimeContext<IMultiMirrorFileDownloader>>
            _createMultiMirrorFileDownloader;
        readonly Func<IMultiMirrorFileDownloader, ExportLifetimeContext<IFileQueueDownloader>>
            _createQueueDownloader;
        readonly IFileDownloader _downloader;

        public FileDownloadHelper(IFileDownloader downloader,
            Func<IMirrorSelector, ExportLifetimeContext<IMultiMirrorFileDownloader>> createMultiMirrorFileDownloader,
            Func<IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>> createMirrorSelector,
            Func<int, IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>> createMirrorSelectorWithLimit,
            Func<IMultiMirrorFileDownloader, ExportLifetimeContext<IFileQueueDownloader>> createQueueDownloader) {
            _downloader = downloader;
            _createMirrorSelector = createMirrorSelector;
            _createMirrorSelectorWithLimit = createMirrorSelectorWithLimit;
            _createMultiMirrorFileDownloader = createMultiMirrorFileDownloader;
            _createQueueDownloader = createQueueDownloader;
        }

        public Task DownloadFileAsync(Uri uri, IAbsoluteFilePath localFile) {
            localFile.MakeSureParentPathExists();
            return _downloader.DownloadAsync(uri, localFile);
        }

        public async Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            IReadOnlyCollection<Uri> remotes,
            CancellationToken token,
            int limit) {
            using (var scoreMirrorSelector = _createMirrorSelectorWithLimit(limit, remotes)) {
                await
                    DownloadFileAsync(remoteFile, destinationPath, scoreMirrorSelector, token, null,
                        RepositoryRemote.CalculateHttpFallbackAfter(limit)).ConfigureAwait(false);
            }
        }

        public async Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            IReadOnlyCollection<Uri> remotes, CancellationToken token, int limit,
            Func<IAbsoluteFilePath, bool> confirmValidity) {
            using (var scoreMirrorSelector = _createMirrorSelectorWithLimit(limit, remotes)) {
                await
                    DownloadFileAsync(remoteFile, destinationPath, scoreMirrorSelector, token, confirmValidity,
                        RepositoryRemote.CalculateHttpFallbackAfter(limit))
                        .ConfigureAwait(false);
            }
        }

        public async Task DownloadFilesAsync(IReadOnlyCollection<Uri> remotes, StatusRepo sr,
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destination) {
            sr.Action = RepoStatus.Downloading;
            using (var scoreMirrorSelector = _createMirrorSelector(remotes)) {
                await
                    DownloadFilesAsync(sr, transferStatuses, destination, scoreMirrorSelector)
                        .ConfigureAwait(false);
            }
        }

        // NOTE: Must manually control AllowHttpZsyncFallbackAfter - generally based on the limit!
        public async Task DownloadFilesAsync(IReadOnlyCollection<Uri> remotes, StatusRepo sr,
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destination,
            int limit) {
            sr.Action = RepoStatus.Downloading;
            using (var scoreMirrorSelector = _createMirrorSelectorWithLimit(limit, remotes)) {
                await
                    DownloadFilesAsync(sr, transferStatuses, destination, scoreMirrorSelector)
                        .ConfigureAwait(false);
            }
        }

        async Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            ExportLifetimeContext<IMirrorSelector> scoreMirrorSelector, CancellationToken token) {
            destinationPath.MakeSurePathExists();
            using (var dl = _createMultiMirrorFileDownloader(scoreMirrorSelector.Value)) {
                await
                    dl.Value.DownloadAsync(new MultiMirrorFileDownloadSpec(remoteFile,
                        destinationPath.GetChildFileWithName(remoteFile)) {CancellationToken = token}, token)
                        .ConfigureAwait(false);
            }
        }

        async Task DownloadFileAsync(string remoteFile, IAbsoluteDirectoryPath destinationPath,
            ExportLifetimeContext<IMirrorSelector> scoreMirrorSelector, CancellationToken token,
            Func<IAbsoluteFilePath, bool> confirmValidity, int zsyncHttpFallbackAfter) {
            destinationPath.MakeSurePathExists();
            using (var dl = _createMultiMirrorFileDownloader(scoreMirrorSelector.Value)) {
                await
                    dl.Value.DownloadAsync(new MultiMirrorFileDownloadSpec(remoteFile,
                        destinationPath.GetChildFileWithName(remoteFile), confirmValidity) {
                            CancellationToken = token,
                            Progress = new TransferStatus(remoteFile) {ZsyncHttpFallbackAfter = zsyncHttpFallbackAfter}
                        },
                        token).ConfigureAwait(false);
            }
        }

        async Task DownloadFilesAsync(StatusRepo sr,
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destinationPath, ExportLifetimeContext<IMirrorSelector> scoreMirrorSelector) {
            destinationPath.MakeSurePathExists();
            sr.Total = transferStatuses.Count;
            using (var multiMirrorFileDownloader = _createMultiMirrorFileDownloader(scoreMirrorSelector.Value))
            using (var multi = _createQueueDownloader(multiMirrorFileDownloader.Value)) {
                await multi.Value
                    .DownloadAsync(CreateFileQueueSpec(transferStatuses, destinationPath), sr.CancelToken)
                    .ConfigureAwait(false);
            }
        }

        static FileQueueSpec CreateFileQueueSpec(
            IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> transferStatuses,
            IAbsoluteDirectoryPath destination) {
            return new FileQueueSpec(transferStatuses, destination);
        }
    }
}