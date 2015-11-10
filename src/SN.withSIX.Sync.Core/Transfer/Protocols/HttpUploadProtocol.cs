// <copyright company="SIX Networks GmbH" file="HttpUploadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public class HttpUploadProtocol : UploadProtocol
    {
        static readonly IEnumerable<string> schemes = new[] {"ftp", "http", "https"};
        static readonly TimeSpan timeout = TimeSpan.FromSeconds(60);
        readonly ExportFactory<IWebClient> _webClientFactory;

        public HttpUploadProtocol(ExportFactory<IWebClient> webClientFactory) {
            _webClientFactory = webClientFactory;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        [Obsolete("Use async variant")]
        public override void Upload(TransferSpec spec) {
            spec.Progress.Tries++;
            UploadAsync(spec).WaitAndUnwrapException();
        }

        public override async Task UploadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            using (var webClient = _webClientFactory.CreateExport())
            using (SetupTransferProgress(webClient.Value, spec.Progress))
                await TryUploadAsync(spec, webClient.Value).ConfigureAwait(false);
        }

        static UploadException CreateTimeoutException(TransferSpec spec, Exception e) {
            return new UploadSoftException("", null, null, new TimeoutException(
                String.Format("The operation timedout in {0}. {1}", timeout, CreateTransferExceptionMessage(spec)), e));
        }

        static async Task TryUploadAsync(TransferSpec spec, IWebClient webClient) {
            try {
                if (!string.IsNullOrWhiteSpace(spec.Uri.UserInfo))
                    webClient.SetAuthInfo(spec.Uri);
                using (webClient.HandleCancellationToken(spec))
                    await webClient.UploadFileTaskAsync(spec.Uri, spec.LocalFile.ToString()).ConfigureAwait(false);
            } catch (OperationCanceledException e) {
                throw CreateTimeoutException(spec, e);
            } catch (WebException e) {
                var cancelledEx = e.InnerException as OperationCanceledException;
                if (cancelledEx != null)
                    throw CreateTimeoutException(spec, cancelledEx);
                if (e.Status == WebExceptionStatus.RequestCanceled)
                    throw CreateTimeoutException(spec, e);

                GenerateUploadException(spec, e);
            }
        }

        static void GenerateUploadException(TransferSpec spec, WebException e) {
            // TODO: Or should we rather abstract this away into the downloader exceptions instead?
            var r = e.Response as HttpWebResponse;
            if (r != null) {
                throw new HttpUploadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e) {
                    StatusCode = r.StatusCode
                };
            }
            var r2 = e.Response as FtpWebResponse;
            if (r2 != null) {
                throw new FtpUploadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e) {
                    StatusCode = r2.StatusCode
                };
            }
            throw new UploadException(e.Message + ". " + CreateTransferExceptionMessage(spec), e);
        }

        static Timer SetupTransferProgress(IWebClient webClient, ITransferProgress transferProgress) {
            var lastTime = Tools.Generic.GetCurrentUtcDateTime;
            long lastBytes = 0;

            webClient.UploadProgressChanged +=
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
                    lastTime = now;
                    lastBytes = bytes;
                };

            webClient.UploadFileCompleted += (sender, args) => { transferProgress.Completed = true; };

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