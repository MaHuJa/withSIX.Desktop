// <copyright company="SIX Networks GmbH" file="DataDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class DataDownloader : IDataDownloader
    {
        readonly IFileDownloader _downloader;

        public DataDownloader(IFileDownloader downloader) {
            _downloader = downloader;
        }

        public byte[] Download(Uri uri) {
            using (var tmpFile = new TmpFileCreated()) {
                _downloader.Download(uri, tmpFile.FilePath);
                return File.ReadAllBytes(tmpFile.FilePath.ToString());
            }
        }

        public async Task<byte[]> DownloadAsync(Uri uri) {
            using (var tmpFile = new TmpFileCreated()) {
                await _downloader.DownloadAsync(uri, tmpFile.FilePath).ConfigureAwait(false);
                return File.ReadAllBytes(tmpFile.FilePath.ToString());
            }
        }

        public byte[] Download(Uri uri, ITransferProgress progress) {
            using (var tmpFile = new TmpFileCreated()) {
                _downloader.Download(uri, tmpFile.FilePath, progress);
                return File.ReadAllBytes(tmpFile.FilePath.ToString());
            }
        }

        public async Task<byte[]> DownloadAsync(Uri uri, ITransferProgress progress) {
            using (var tmpFile = new TmpFileCreated()) {
                await _downloader.DownloadAsync(uri, tmpFile.FilePath, progress).ConfigureAwait(false);
                return File.ReadAllBytes(tmpFile.FilePath.ToString());
            }
        }
    }
}