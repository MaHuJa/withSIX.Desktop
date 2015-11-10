// <copyright company="SIX Networks GmbH" file="FileDownloadManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Protocols;

namespace SN.withSIX.Sync.Core.Legacy
{
    [Obsolete]
    public class FileDownloadManager : IEnableLogging
    {
        const double StatusInterval = 150;
        readonly IFileDownloader _downloader;
        readonly List<Type> _exceptionTypes = new List<Type>();
        readonly StatusRepo _statusRepo;

        public FileDownloadManager(HostPicker hostPicker, StatusRepo repo, IFileDownloader downloader) {
            Contract.Requires<ArgumentNullException>(hostPicker != null);
            Contract.Requires<ArgumentNullException>(repo != null);
            Contract.Requires<ArgumentNullException>(downloader != null);

            HostPicker = hostPicker;
            _statusRepo = repo;
            _downloader = downloader;
        }

        public FileDownloadManager(StatusRepo repo, IEnumerable<Uri> hosts,
            MultiThreadingSettings multiThreadingSettings, IFileDownloader downloader,
            Func<ExportLifetimeContext<IHostChecker>> hostChecker)
            : this(
                new HostPicker(
                    hosts.Select(
                        x => new Uri(x.ToString().Replace("http://", "zsync://").Replace("https://", "zsyncs://"))),
                    multiThreadingSettings,
                    hostChecker), repo, downloader) {}

        public HostPicker HostPicker { get; protected set; }

        public Task Process<T>(T[] items, Action<T> act, int? maxThreads = null) {
            this.Logger().Info("Processing: {0}", String.Join(", ", items));

            if (maxThreads == null)
                maxThreads = HostPicker.MultiThreadingSettings.MaxThreads;

            return StartQueue(items, act, maxThreads.Value);
        }

        public Task ProcessAsync<T>(T[] items, CancellationTokenSource token, Func<T, Task> act, int? maxThreads = null) {
            this.Logger().Info("Processing: {0}", String.Join(", ", items));

            if (maxThreads == null)
                maxThreads = HostPicker.MultiThreadingSettings.MaxThreads;

            return StartConcurrentQueue(items, act, maxThreads.Value, token.Token);
        }

        public async Task ProcessAsync<T>(T[] items, Func<T, Task> act, int? maxThreads = null) {
            using (var token = new CancellationTokenSource())
                await ProcessAsync(items, token, act, maxThreads).ConfigureAwait(false);
        }

        public async Task FetchFiles(string[] files, string destination) {
            _statusRepo.Action = RepoStatus.Downloading;
            using (var token = new CancellationTokenSource()) {
                await
                    ProcessAsync(files, token,
                        x => FetchFileAsyncWithStatus(x, x, destination, token)).ConfigureAwait(false);
            }
        }

        public async Task FetchFiles(FileFetchInfo[] files, string destination) {
            _statusRepo.Action = RepoStatus.Downloading;
            using (var token = new CancellationTokenSource()) {
                await
                    ProcessAsync(files, token,
                        x =>
                            FetchFileAsyncWithStatus(x.DisplayName, x.FilePath, destination, token))
                        .ConfigureAwait(false);
            }
        }

        public async Task FetchFileAsync(Spec spec) {
            if (_statusRepo.Aborted)
                throw new AbortedException();

            TryPickHost(spec);
            var done = false;
            while (!done && spec.CurrentHost != null && !_statusRepo.Aborted) {
                spec.Status.Info = spec.CurrentHost.ToString();
                this.Logger().Info("Fetching {0} @ {1}", spec.File, spec.CurrentHost);
                done = await TryDownloadFile(spec).ConfigureAwait(false);
                if (!done)
                    TryPickHost(spec);
            }

            if (done)
                spec.Status.EndOutput(spec.FullPath.ToString());
            else {
                spec.Status.FailOutput(spec.FullPath.ToString());
                throw new HostListExhausted(String.Empty, spec.LastException);
            }
        }

        public Spec GetSpec(string file, IAbsoluteFilePath fullPath, IStatus status) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(file));
            Contract.Requires<ArgumentNullException>(fullPath != null);
            Contract.Requires<ArgumentNullException>(status != null);

            status.StartOutput(fullPath.ToString());
            status.Action = RepoStatus.Downloading;

            var spec = new Spec(file, fullPath, status);
            return spec;
        }

        async Task<bool> TryDownloadFile(Spec spec) {
            var done = false;
            spec.FullPath.MakeSureParentPathExists();
            try {
                if (spec.CurrentHost != null) {
                    await
                        _downloader.DownloadAsync(spec.GetUri(), spec.FullPath, spec.Status)
                            .ConfigureAwait(false);
                    done = true;
                    TryCleanTempFiles(spec.FullPath);
                }
            } catch (RsyncSoftException e) {
                spec.LastException = e;
                this.Logger().Warn("Failed transfer of {0} from {1}: {2}", spec.File, spec.CurrentHost, e.Message);
                this.Logger().Warn(e.Output);
            } catch (ZsyncSoftException e) {
                spec.LastException = e;
                this.Logger().Warn("Failed transfer of {0} from {1}: {2}", spec.File, spec.CurrentHost, e.Message);
                this.Logger().Warn(e.Output);
                if (e is ZsyncIncompatibleException)
                    HostPicker.ZsyncIncompatHost(spec.CurrentHost);
            } catch (DownloadSoftException e) {
                spec.LastException = e;
                this.Logger().Warn("Failed transfer of {0} from {1}: {2}", spec.File, spec.CurrentHost, e.Message);
                this.Logger().Warn(e.Output);
            } catch (DownloadException e) {
                spec.LastException = e;
                this.Logger().FormattedWarnException(e);
                this.Logger().Warn("Failed transfer of {0} from {1}: {2}", spec.File, spec.CurrentHost, e.Message);
                this.Logger().Warn(e.Output);
                if (AddExceptionType(e.GetType()))
                    await UserError.Throw(new InformationalUserError(e, "Error occurred during download", null));
            } catch (TimeoutException e) {
                spec.LastException = e;
                this.Logger().FormattedWarnException(e);
                this.Logger()
                    .Warn("Failed transfer of {0} from {1}: {2} (Timeout)", spec.File, spec.CurrentHost, e.Message);
            } catch (SocketException e) {
                spec.LastException = e;
                this.Logger().FormattedWarnException(e);
                this.Logger()
                    .Warn("Failed transfer of {0} from {1}: {2} (ErrorCode: {3}, Native: {4}, Socket: {5})", spec.File,
                        spec.CurrentHost, e.Message, e.ErrorCode, e.NativeErrorCode, e.SocketErrorCode);
            } catch (WebException e) {
                spec.LastException = e;
                this.Logger()
                    .Warn("Failed transfer of {0} from {1}: {2} (Status: {3}, Type: {4}, Length: {5})", spec.File,
                        spec.CurrentHost, e.Message, e.Status, e.Response == null ? null : e.Response.ContentType,
                        e.Response == null ? 0 : e.Response.ContentLength);
                this.Logger().FormattedWarnException(e);
            } catch (Exception e) {
                spec.LastException = e;
                this.Logger().FormattedWarnException(e);
                if (AddExceptionType(e.GetType()))
                    await UserError.Throw(new InformationalUserError(e, "Error occurred during download", null));
            }
            return done;
        }

        bool TryPickHost(Spec spec) {
            try {
                spec.CurrentHost = HostPicker.Pick(spec.CurrentHost);
                return true;
            } catch (HostListExhausted) {
                spec.CurrentHost = null;
                return false;
            }
        }

        Task StartQueue<T>(ICollection<T> items, Action<T> act, int maxThreads) {
            _statusRepo.Total = items.Count + _statusRepo.Items.Count;
            return TryStartQueue(items, act, maxThreads);
        }

        async Task TryStartQueue<T>(IEnumerable<T> items, Action<T> act, int maxThreads) {
            try {
                await items.SimpleQueue(maxThreads, act).ConfigureAwait(false);
            } catch (AggregateException e) {
                this.Logger().FormattedErrorException(e);
                var f = e.Flatten();
                throw f.InnerExceptions.First();
            } finally {
                _statusRepo.Finish();
            }
        }

        Task StartConcurrentQueue<T>(ICollection<T> items, Func<T, Task> act, int maxThreads, CancellationToken token) {
            _statusRepo.Total = items.Count + _statusRepo.Items.Count;
            return TryStartConcurrentQueue(items, act, maxThreads, token);
        }

        async Task TryStartConcurrentQueue<T>(IEnumerable<T> items, Func<T, Task> act, int maxThreads,
            CancellationToken token) {
            try {
                await items.StartConcurrentTaskQueue(token, act, () => maxThreads).ConfigureAwait(false);
            } catch (AggregateException e) {
                this.Logger().FormattedErrorException(e);
                var f = e.Flatten();
                throw f.InnerExceptions.First();
            } finally {
                _statusRepo.Finish();
            }
        }

        Task FetchFileAsyncWithStatus(string file, string filePath, string destination, CancellationTokenSource token) {
            var status = new Status.Status(file, _statusRepo) {RealObject = filePath};
            var f = Path.Combine(destination, filePath).ToAbsoluteFilePath();
            status.StartOutput(f.ToString());
            return TryFetchFileAsyncWithStatus(GetSpec(file, f, status), token);
        }

        async Task TryFetchFileAsyncWithStatus(Spec spec, CancellationTokenSource token) {
            try {
                await
                    FetchFileAsync(spec).ConfigureAwait(false);
                spec.Status.EndOutput(spec.FullPath.ToString());
            } catch (HostListExhausted) {
                spec.Status.FailOutput(spec.FullPath.ToString());
                token.Cancel();
                throw;
            } catch (AbortedException) {
                spec.Status.FailOutput(spec.FullPath.ToString());
                token.Cancel();
                throw;
            } catch (Exception) {
                spec.Status.FailOutput(spec.FullPath.ToString());
                throw;
            }
        }

        void TryCleanTempFiles(IAbsoluteFilePath fullPath) {
            foreach (var file in Repository.TempExtensions
                .Select(x => fullPath + x)
                .Where(File.Exists)) {
                try {
                    Tools.FileUtil.Ops.DeleteWithRetry(file);
                } catch (Exception e) {
                    this.Logger().FormattedDebugException(e);
                }
            }
        }

        bool AddExceptionType(Type eType) {
            lock (_exceptionTypes) {
                if (_exceptionTypes.Any(x => x == eType))
                    return false;
                _exceptionTypes.Add(eType);
                return true;
            }
        }
    }

    public struct FileFetchInfo
    {
        public string DisplayName;
        public string FilePath;

        public FileFetchInfo(string filePath, string displayName = null) {
            FilePath = filePath;
            DisplayName = displayName ?? filePath;
        }

        public override string ToString() {
            return DisplayName ?? String.Empty;
        }
    }

    public class Spec
    {
        public Spec(string file, IAbsoluteFilePath fullPath, IStatus status) {
            File = file;
            FullPath = fullPath;
            Status = status;
        }

        public Exception LastException { get; set; }
        public IStatus Status { get; }
        public IAbsoluteFilePath FullPath { get; }
        public string File { get; }
        public Uri CurrentHost { get; set; }

        public Uri GetUri() {
            var hostUri = CurrentHost;
            return Tools.Transfer.JoinUri(hostUri, Tools.Transfer.EncodePathIfRequired(hostUri, File));
        }
    }
}