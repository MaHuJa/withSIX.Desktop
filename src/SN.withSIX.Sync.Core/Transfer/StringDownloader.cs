// <copyright company="SIX Networks GmbH" file="StringDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class StringDownloader : IStringDownloader
    {
        readonly IFileDownloader _downloader;

        public StringDownloader(IFileDownloader downloader) {
            _downloader = downloader;
        }

        public string Download(Uri uri) {
            using (var tmpFile = new TmpFileCreated()) {
                _downloader.Download(uri, tmpFile.FilePath);
                return File.ReadAllText(tmpFile.FilePath.ToString());
            }
        }

        public async Task<string> DownloadAsync(Uri uri) {
            using (var tmpFile = new TmpFileCreated()) {
                await _downloader.DownloadAsync(uri, tmpFile.FilePath).ConfigureAwait(false);
                return File.ReadAllText(tmpFile.FilePath.ToString());
            }
        }

        public string Download(Uri uri, ITransferProgress progress) {
            using (var tmpFile = new TmpFileCreated()) {
                _downloader.Download(uri, tmpFile.FilePath, progress);
                return File.ReadAllText(tmpFile.FilePath.ToString());
            }
        }

        public async Task<string> DownloadAsync(Uri uri, ITransferProgress progress) {
            using (var tmpFile = new TmpFileCreated()) {
                await _downloader.DownloadAsync(uri, tmpFile.FilePath, progress).ConfigureAwait(false);
                return File.ReadAllText(tmpFile.FilePath.ToString());
            }
        }
    }
}