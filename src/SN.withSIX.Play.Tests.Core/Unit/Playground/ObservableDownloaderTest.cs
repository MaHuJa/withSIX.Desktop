using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Timers;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground
{
    public abstract class DownloadProtocol2 : TransferProtocol
    {
        protected static void VerifyIfNeeded(TransferSpec spec, IAbsoluteFilePath localFile) {
            if (spec.Verification == null || spec.Verification(localFile))
                return;
            Tools.FileUtil.Ops.DeleteFile(localFile);
            throw new VerificationError(localFile.ToString());
        }

        public abstract Task DownloadAsync(TransferSpec spec, Action<int, double, int> progressReporting);
    }

    public class HttpDownloadProtocol2 : DownloadProtocol2
    {
        static readonly TimeSpan timeout = TimeSpan.FromSeconds(60);
        static readonly IEnumerable<string> schemes = new[] { "http", "https", "ftp" };
        readonly Tools.FileTools.IFileOps _fileOps;
        readonly ExportFactory<IWebClient> _webClientFactory;

        public HttpDownloadProtocol2(ExportFactory<IWebClient> webClientFactory, Tools.FileTools.IFileOps fileOps) {
            _webClientFactory = webClientFactory;
            _fileOps = fileOps;
        }

        public override IEnumerable<string> Schemes {
            get { return schemes; }
        }

        public override async Task DownloadAsync(TransferSpec spec, Action<int, double, int> progressReporting) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            var obs = new Subject<ITransferStatus2>();
            var act =
                new Action<int, double, int>(
                    (bytesReceived, progress, speed) => obs.OnNext(new TransferStatus2(progress, bytesReceived, speed)));
            //DoDownload(spec).ToObservable().Catch((ex, e2) => obs.OnError(ex));
        }

        async Task DoDownload(TransferSpec spec) {
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
                            transferProgress.Speed = (long)(bytesChange / (timeSpan.TotalMilliseconds / 1000.0));
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

    public class TransferStatus2 : ITransferStatus2
    {
        public TransferStatus2(double progress, long bytesReceived, int speedInBytes) {
            Progress = progress;
            BytesReceived = bytesReceived;
            SpeedInBytes = speedInBytes;
        }
        public long BytesReceived { get; private set; }
        public int SpeedInBytes { get; private set; }
        public double Progress { get; private set; }
    }

    public interface ITransferStatus2
    {
        long BytesReceived { get; }
        int SpeedInBytes { get; }
        double Progress { get; }
    }
}
