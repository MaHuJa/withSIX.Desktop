// <copyright company="SIX Networks GmbH" file="ZsyncDownloadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    [DoNotObfuscate]
    public class ZsyncException : DownloadException
    {
        public ZsyncException(string message, string output = null, string parameters = null, Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    [DoNotObfuscate]
    public class ZsyncSoftException : ZsyncException
    {
        public ZsyncSoftException(string message, string output = null, string parameters = null, Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    [DoNotObfuscate]
    public class ZsyncLoopDetectedException : ZsyncSoftException
    {
        public ZsyncLoopDetectedException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    [DoNotObfuscate]
    public class ZsyncIncompatibleException : ZsyncSoftException
    {
        public ZsyncIncompatibleException(string message, string output = null, string parameters = null,
            Exception inner = null)
            : base(message, output, parameters, inner) {}
    }

    [DoNotObfuscate]
    public class ZsyncDownloadProtocol : DownloadProtocol
    {
        static readonly string[] schemes = {"zsync", "zsyncs"};
        readonly IZsyncLauncher _zsyncLauncher;

        public ZsyncDownloadProtocol(IZsyncLauncher zsyncLauncher) {
            _zsyncLauncher = zsyncLauncher;
        }

        public override IEnumerable<string> Schemes
        {
            get { return schemes; }
        }

        public override async Task DownloadAsync(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            spec.Progress.ResetZsyncLoopInfo();
            ProcessExitResult(await _zsyncLauncher.RunAndProcessAsync(spec.Progress,
                new Uri(FixUrl(spec.Uri)), spec.LocalFile.ToString(), spec.CancellationToken).ConfigureAwait(false),
                spec);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        public override void Download(TransferSpec spec) {
            spec.Progress.Tries++;
            ConfirmSchemeSupported(spec.Uri.Scheme);
            spec.Progress.ResetZsyncLoopInfo();
            ProcessExitResult(_zsyncLauncher.RunAndProcess(spec.Progress,
                new Uri(FixUrl(spec.Uri)), spec.LocalFile.ToString(), spec.CancellationToken), spec);
            VerifyIfNeeded(spec, spec.LocalFile);
        }

        static void ProcessExitResult(ProcessExitResultWithOutput result, TransferSpec spec) {
            if (spec.Progress.ZsyncIncompatible) {
                throw new ZsyncIncompatibleException(
                    String.Format("Zsync Incompatible (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError,
                    result.StartInfo.Arguments);
            }

            switch (result.ExitCode) {
            case 0:
                break;
            case -1: {
                if (spec.Progress.ZsyncLoopCount >= 2) {
                    throw new ZsyncLoopDetectedException(
                        String.Format("Loop detected, aborted transfer (PID: {0}, Status: {1}). {2}", result.Id,
                            result.ExitCode, CreateTransferExceptionMessage(spec)),
                        result.StandardOutput + result.StandardError,
                        result.StartInfo.Arguments);
                }

                throw new ZsyncSoftException(
                    String.Format("Aborted/Killed (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError,
                    result.StartInfo.Arguments);
            }
            case 1:
                throw new ZsyncSoftException(
                    String.Format(
                        "Could not retrieve file due to protocol error (not a zsync file?) (PID: {0}, Status: {1}). {2}",
                        result.Id, result.ExitCode, CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 2:
                throw new ZsyncSoftException(
                    String.Format("Connection reset (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            case 3:
                throw new ZsyncSoftException(
                    String.Format("Could not retrieve file (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            default:
                throw new ZsyncException(
                    String.Format("Did not exit gracefully (PID: {0}, Status: {1}). {2}", result.Id, result.ExitCode,
                        CreateTransferExceptionMessage(spec)),
                    result.StandardOutput + result.StandardError, result.StartInfo.Arguments);
            }
        }

        static string FixUrl(Uri uri) {
            return uri.Scheme == "zsync" || uri.Scheme == "zsyncs"
                ? uri.ReplaceZsyncProtocol() + ".zsync"
                : uri + ".zsync";
        }
    }

    public class ZsyncDownloadWithHttpFallbackProtocol : ZsyncDownloadProtocol
    {
        readonly IHttpDownloadProtocol _httpDownloader;

        public ZsyncDownloadWithHttpFallbackProtocol(IZsyncLauncher zsyncLauncher,
            IHttpDownloadProtocol httpDownloader) : base(zsyncLauncher) {
            _httpDownloader = httpDownloader;
        }

        public override void Download(TransferSpec spec) {
            TryDownload(spec);
        }

        public override Task DownloadAsync(TransferSpec spec) {
            return TryDownloadAsync(spec);
        }

        async Task TryDownloadAsync(TransferSpec spec) {
            Exception retryEx = null;
            try {
                await base.DownloadAsync(spec).ConfigureAwait(false);
            } catch (ZsyncIncompatibleException e) {
                retryEx = e;
            } catch (ZsyncLoopDetectedException e) {
                retryEx = e;
            } catch (ZsyncSoftException e) {
                var progress = spec.Progress;
                if (progress != null && !AllowZsyncFallback(progress))
                    throw;
                retryEx = e;
            }
            if (retryEx != null)
                await TryRegularHttpDownloadAsync(spec, retryEx).ConfigureAwait(false);
        }

        static bool AllowZsyncFallback(ITransferProgress progress) {
            return progress.ZsyncHttpFallback || progress.Tries > progress.ZsyncHttpFallbackAfter;
        }

        void TryDownload(TransferSpec spec) {
            try {
                base.Download(spec);
            } catch (ZsyncIncompatibleException e) {
                TryRegularHttpDownload(spec, e);
            } catch (ZsyncLoopDetectedException e) {
                TryRegularHttpDownload(spec, e);
            } catch (ZsyncSoftException e) {
                var progress = spec.Progress;
                if (progress != null && !AllowZsyncFallback(progress))
                    throw;
                TryRegularHttpDownload(spec, e);
            }
        }

        protected virtual void TryRegularHttpDownload(TransferSpec spec, Exception exception) {
            spec.Progress.Tries++;
            _httpDownloader.Download(new FileDownloadSpec(spec.Uri.ReplaceZsyncProtocol(), spec.LocalFile, spec.Progress));
        }

        protected virtual Task TryRegularHttpDownloadAsync(TransferSpec spec, Exception exception) {
            spec.Progress.Tries++;
            return
                _httpDownloader.DownloadAsync(new FileDownloadSpec(spec.Uri.ReplaceZsyncProtocol(), spec.LocalFile,
                    spec.Progress));
        }
    }

    static class UriExtensions
    {
        internal static string ReplaceZsyncProtocol(this Uri uri) {
            return uri.ToString().Replace("zsync://", "http://").Replace("zsyncs://", "https://");
        }
    }

    public class LoggingZsyncDownloadWithHttpFallbackProtocol : ZsyncDownloadWithHttpFallbackProtocol, IEnableLogging
    {
        public LoggingZsyncDownloadWithHttpFallbackProtocol(IZsyncLauncher zsyncLauncher,
            IHttpDownloadProtocol httpDownloader) : base(zsyncLauncher, httpDownloader) {}

        protected override void TryRegularHttpDownload(TransferSpec spec, Exception e) {
            LogMessage(spec, e);
            base.TryRegularHttpDownload(spec, e);
        }

        protected override Task TryRegularHttpDownloadAsync(TransferSpec spec, Exception e) {
            LogMessage(spec, e);
            return base.TryRegularHttpDownloadAsync(spec, e);
        }

        void LogMessage(TransferSpec spec, Exception e) {
            var tfex = e as DownloadException;
            string o = null;
            if (tfex != null)
                o = "\nOutput: " + tfex.Output;
            this.Logger()
                .Warn("Performing http fallback for {0} due to {1} ({2}){3}", spec.Uri.AuthlessUri(), e.GetType(),
                    e.Message, o);
        }
    }
}