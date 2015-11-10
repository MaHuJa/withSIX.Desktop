// <copyright company="SIX Networks GmbH" file="LoggingFileDownloaderDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Core.Presentation.Decorators
{
    public class LoggingFileDownloaderDecorator : LoggingFileTransferDecorator, IFileDownloader
    {
        readonly IFileDownloader _downloader;

        public LoggingFileDownloaderDecorator(IFileDownloader downloader) {
            _downloader = downloader;
        }

        public void Download(FileDownloadSpec spec) {
            Wrap(() => _downloader.Download(spec), spec);
        }

        public Task DownloadAsync(FileDownloadSpec spec) {
            return Wrap(() => _downloader.DownloadAsync(spec), spec);
        }

        protected override void OnStart(TransferSpec spec) {
            this.Logger().Info("Started download of {0} to {1}", spec.Uri, spec.LocalFile);
        }

        protected override void OnFinished(TransferSpec spec) {
            this.Logger().Info("Finished download of {0} to {1}", spec.Uri, spec.LocalFile);
        }

        protected override void OnError(TransferSpec spec, Exception e) {
            this.Logger()
                .Error("Failed download of {0} to {1}, try {2} ({3})\nOutput: {4}\n\nError report: {5}", spec.Uri,
                    spec.LocalFile, spec.Progress.Tries, e.Message, spec.Progress.Output, e.Format(1));
        }
    }
}