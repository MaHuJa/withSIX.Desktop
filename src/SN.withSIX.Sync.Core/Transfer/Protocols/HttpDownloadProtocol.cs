// <copyright company="SIX Networks GmbH" file="HttpDownloadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public interface IHttpDownloadProtocol : IDownloadProtocol {}

    public class HttpDownloadProtocol : DownloadProtocol, IHttpDownloadProtocol
    {
        static readonly TimeSpan timeout = TimeSpan.FromSeconds(60);
        static readonly IEnumerable<string> schemes = new[] {"http", "https", "ftp"};
        readonly Tools.FileTools.IFileOps _fileOps;
        readonly ExportFactory<IWebClient> _webClientFactory;

        public HttpDownloadProtocol(ExportFactory<IWebClient> webClientFactory, Tools.FileTools.IFileOps fileOps) {
            _webClientFactory = webClientFactory;
            _fileOps = fileOps;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        [Obsolete("Use async variant")]
        public override void Download(TransferSpec spec) {
            spec.Progress.Tries++;
            // Wrapping in Task.Run could solve deadlock issues
            Task.Run(() => DownloadAsync(spec)).WaitAndUnwrapException();
        }

        public override async Task DownloadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            using (var webClient = _webClientFactory.CreateExport())
            using (SetupTransferProgress(webClient.Value, spec.Progress))
                await TryDownloadAsync(spec, webClient.Value, GetTmpFile(spec)).ConfigureAwait(false);
        }

        static IAbsoluteFilePath GetTmpFile(TransferSpec spec) {
            return (spec.LocalFile + ".sixtmp").ToAbsoluteFilePath();
        }

        static DownloadException CreateTimeoutException(TransferSpec spec, Exception e) {
            return new DownloadSoftException("", null, null, new TimeoutException(
                String.Format("The operation timedout in {0}. {1}", timeout, CreateTransferExceptionMessage(spec)), e));
        }

        async Task TryDownloadAsync(TransferSpec spec, IWebClient webClient, IAbsoluteFilePath tmpFile) {
            try {
                tmpFile.RemoveReadonlyWhenExists();
                if (!string.IsNullOrWhiteSpace(spec.Uri.UserInfo))
                    webClient.SetAuthInfo(spec.Uri);
                using (webClient.HandleCancellationToken(spec))
                    await webClient.DownloadFileTaskAsync(spec.Uri, tmpFile.ToString()).ConfigureAwait(false);
                VerifyIfNeeded(spec, tmpFile);
                _fileOps.Move(tmpFile, spec.LocalFile);
            } catch (OperationCanceledException e) {
                _fileOps.DeleteIfExists(tmpFile.ToString());
                throw CreateTimeoutException(spec, e);
            } catch (WebException e) {
                _fileOps.DeleteIfExists(tmpFile.ToString());
                var cancelledEx = e.InnerException as OperationCanceledException;
                if (cancelledEx != null)
                    throw CreateTimeoutException(spec, cancelledEx);
                if (e.Status == WebExceptionStatus.RequestCanceled)
                    throw CreateTimeoutException(spec, e);

                GenerateDownloadException(spec, e);
            }
        }

        static void GenerateDownloadException(TransferSpec spec, WebException e) {
            // TODO: Or should we rather abstract this away into the downloader exceptions instead?
            var r = e.Response as HttpWebResponse;
            if (r != null) {
                throw new HttpDownloadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e) {
                    StatusCode = r.StatusCode
                };
            }
            var r2 = e.Response as FtpWebResponse;
            if (r2 != null) {
                throw new FtpDownloadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e) {
                    StatusCode = r2.StatusCode
                };
            }
            throw new DownloadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e);
        }

        static Timer SetupTransferProgress(IWebClient webClient, ITransferProgress transferProgress) {
            var lastTime = Tools.Generic.GetCurrentUtcDateTime;
            long lastBytes = 0;

            transferProgress.Progress = 0;
            transferProgress.FileSizeTransfered = 0;

            webClient.DownloadProgressChanged +=
                (sender, args) => {
                    var bytes = args.BytesReceived;
                    var now = Tools.Generic.GetCurrentUtcDateTime;

                    transferProgress.Progress = args.ProgressPercentage;
                    transferProgress.FileSizeTransfered = bytes;

                    if (lastBytes != 0) {
                        var timeSpan = now - lastTime;
                        var bytesChange = bytes - lastBytes;

                        if (timeSpan.TotalMilliseconds > 0)
                            transferProgress.Speed = (long) (bytesChange/(timeSpan.TotalMilliseconds/1000.0));
                    }

                    lastBytes = bytes;
                    lastTime = now;
                };

            webClient.DownloadFileCompleted += (sender, args) => { transferProgress.Completed = true; };

            var timer = new TimerWithElapsedCancellation(500, () => {
                if (!transferProgress.Completed
                    && Tools.Generic.LongerAgoThan(lastTime, timeout)) {
                    webClient.CancelAsync();
                    return false;
                }
                return !transferProgress.Completed;
            });
            return timer;
        }
    }
}