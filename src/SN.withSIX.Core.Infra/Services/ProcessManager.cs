// <copyright company="SIX Networks GmbH" file="ProcessManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Services.Infrastructure;
using Timer = System.Timers.Timer;

namespace SN.withSIX.Core.Infra.Services
{
    public static class ProcessExtensions
    {
        public static ProcessManager.ProcessState MonitorProcessOutput(this Process process) {
            Contract.Requires<ArgumentNullException>(process != null);

            var state = new ProcessManager.ProcessState();

            if (!process.StartInfo.RedirectStandardOutput)
                throw new InvalidOperationException("Not redirected output");
            if (!process.StartInfo.RedirectStandardError)
                throw new InvalidOperationException("Not redirected error");

            process.OutputDataReceived += (sender, args) => state.UpdateStamp();
            process.ErrorDataReceived += (sender, args) => state.UpdateStamp();

            return state;
        }
    }

    public static class ReactiveProcessExtensions
    {
        public static ProcessManager.ProcessState MonitorProcessOutput(this ReactiveProcess process) {
            Contract.Requires<ArgumentNullException>(process != null);

            var state = new ProcessManager.ProcessState();

            if (!process.StartInfo.RedirectStandardOutput)
                throw new InvalidOperationException("Not redirected output");
            if (!process.StartInfo.RedirectStandardError)
                throw new InvalidOperationException("Not redirected error");

            process.StandardOutputObservable.Subscribe(() => state.UpdateStamp());
            process.StandardErrorObservable.Subscribe(() => state.UpdateStamp());
            return state;
        }
    }

    [DoNotObfuscate]
    public class ProcessManager : IProcessManager, IDisposable, IInfrastructureService
    {
        const int MonitorInterval = 500;
        static readonly TimeSpan defaultMonitorOutputTimeout = TimeSpan.FromSeconds(300);
        static readonly TimeSpan defaultMonitorRespondingTimeout = TimeSpan.FromSeconds(90);
        readonly ISubject<Tuple<ProcessStartInfo, int>, Tuple<ProcessStartInfo, int>> _launched;
        readonly
            ISubject<Tuple<ProcessStartInfo, int, TimeSpan, string>, Tuple<ProcessStartInfo, int, TimeSpan, string>>
            _monitorKilled;
        readonly ISubject<Tuple<ProcessStartInfo, int, string>, Tuple<ProcessStartInfo, int, string>> _monitorStarted;
        readonly ISubject<Tuple<ProcessStartInfo, int, string>, Tuple<ProcessStartInfo, int, string>> _monitorStopped;
        readonly CompositeDisposable _subjects = new CompositeDisposable();
        readonly ISubject<Tuple<ProcessStartInfo, int, int>, Tuple<ProcessStartInfo, int, int>> _terminated;

        public ProcessManager() {
            var launched = new Subject<Tuple<ProcessStartInfo, int>>();
            _launched = Subject.Synchronize(launched);
            Launched = _launched.AsObservable();
            _subjects.Add(launched);

            var killed = new Subject<Tuple<ProcessStartInfo, int, TimeSpan, string>>();
            _monitorKilled = Subject.Synchronize(killed);
            MonitorKilled = _monitorKilled.AsObservable();
            _subjects.Add(killed);

            var started = new Subject<Tuple<ProcessStartInfo, int, string>>();
            _monitorStarted = Subject.Synchronize(started);
            MonitorStarted = _monitorStarted.AsObservable();
            _subjects.Add(started);

            var stopped = new Subject<Tuple<ProcessStartInfo, int, string>>();
            _monitorStopped = Subject.Synchronize(stopped);
            MonitorStopped = _monitorStopped.AsObservable();
            _subjects.Add(stopped);

            var terminated = new Subject<Tuple<ProcessStartInfo, int, int>>();
            _terminated = Subject.Synchronize(terminated);
            Terminated = _terminated.AsObservable();
            _subjects.Add(terminated);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IObservable<Tuple<ProcessStartInfo, int>> Launched { get; }
        public IObservable<Tuple<ProcessStartInfo, int, TimeSpan, string>> MonitorKilled { get; }
        public IObservable<Tuple<ProcessStartInfo, int, string>> MonitorStarted { get; }
        public IObservable<Tuple<ProcessStartInfo, int, string>> MonitorStopped { get; }
        public IObservable<Tuple<ProcessStartInfo, int, int>> Terminated { get; }
        public TimeSpan DefaultMonitorOutputTimeOut
        {
            get { return defaultMonitorOutputTimeout; }
        }
        public TimeSpan DefaultMonitorRespondingTimeOut
        {
            get { return defaultMonitorRespondingTimeout; }
        }

        CompositeDisposable StartProcess(Process process, TimeSpan? monitorOutput = null,
            TimeSpan? monitorResponding = null) {
            process.Validate();
            process.Start();

            _launched.OnNext(Tuple.Create(process.StartInfo, process.Id));

            return SetupMonitoringDisposable(process, monitorOutput, monitorResponding);
        }

        CompositeDisposable SetupMonitoringDisposable(ReactiveProcess process, TimeSpan? monitorOutput,
            TimeSpan? monitorResponding) {
            var disposable = new CompositeDisposable();
            if (monitorOutput.HasValue)
                disposable.Add(MonitorProcessOutput(process, monitorOutput.Value));
            if (monitorResponding.HasValue)
                disposable.Add(MonitorProcessResponding(process, monitorResponding.Value));

            return disposable;
        }

        CompositeDisposable SetupMonitoringDisposable(Process process, TimeSpan? monitorOutput,
            TimeSpan? monitorResponding) {
            var disposable = new CompositeDisposable();
            if (monitorOutput.HasValue)
                disposable.Add(MonitorProcessOutput(process, monitorOutput.Value));
            if (monitorResponding.HasValue)
                disposable.Add(MonitorProcessResponding(process, monitorResponding.Value));

            return disposable;
        }

        ~ProcessManager() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                _subjects.Dispose();
        }

        #region Monitor

        Timer MonitorProcessOutput(ReactiveProcess process, TimeSpan timeout) {
            Contract.Requires<ArgumentNullException>(process != null);
            Contract.Requires<ArgumentNullException>(timeout != null);

            var state = process.MonitorProcessOutput();
            _monitorStarted.OnNext(Tuple.Create(process.StartInfo, process.Id, "Output"));
            return new TimerWithElapsedCancellation(MonitorInterval,
                () => OnOutputMonitorElapsed(process, state, timeout),
                (o, args) => _monitorStopped.OnNext(Tuple.Create(process.StartInfo, process.Id, "Output")));
        }

        Timer MonitorProcessOutput(Process process, TimeSpan timeout) {
            Contract.Requires<ArgumentNullException>(process != null);
            Contract.Requires<ArgumentNullException>(timeout != null);

            var state = process.MonitorProcessOutput();
            _monitorStarted.OnNext(Tuple.Create(process.StartInfo, process.Id, "Output"));
            return new TimerWithElapsedCancellation(MonitorInterval,
                () => OnOutputMonitorElapsed(process, state, timeout),
                (o, args) => _monitorStopped.OnNext(Tuple.Create(process.StartInfo, process.Id, "Output")));
        }

        bool OnOutputMonitorElapsed(Process process, ProcessState state, TimeSpan timeout) {
            if (TryCheckHasExited(process))
                return false;

            var lastResponse = state.Stamp;
            if (!Tools.Generic.LongerAgoThan(lastResponse, timeout))
                return true;

            var lastStreamLengths = state.StreamLengths;
            state.StreamLengths = new Tuple<long, long>(process.StandardOutput.BaseStream.Length,
                process.StandardError.BaseStream.Length);
            if (state.StreamLengths.Item1 != lastStreamLengths.Item1
                || state.StreamLengths.Item2 != lastStreamLengths.Item2) {
                state.UpdateStamp();
                return true;
            }

            TryKillDueToNoOutputReceivedTimeout(process, timeout);
            return false;
        }

        void TryKillDueToNoOutputReceivedTimeout(Process process, TimeSpan timeout) {
            try {
                process.Kill();
            } finally {
                _monitorKilled.OnNext(Tuple.Create(process.StartInfo, process.Id, timeout, "Output"));
            }
        }

        static bool TryCheckHasExited(Process process) {
            try {
                return process.SafeHasExited();
            } catch {
                return true;
            }
        }

        Timer MonitorProcessResponding(Process process, TimeSpan timeout) {
            Contract.Requires<ArgumentNullException>(process != null);
            Contract.Requires<ArgumentNullException>(timeout != null);

            var state = new ProcessState();

            _monitorStarted.OnNext(Tuple.Create(process.StartInfo, process.Id, "Responding"));

            return new TimerWithElapsedCancellation(MonitorInterval, () => OnMonitorElapsed(process, state, timeout),
                (o, args) => _monitorStopped.OnNext(Tuple.Create(process.StartInfo, process.Id, "Responding")));
        }

        bool OnMonitorElapsed(Process process, ProcessState state, TimeSpan timeout) {
            var hasExited = TryCheckHasExited(process);

            if (hasExited)
                return false;

            if (TryCheckResponding(process)) {
                state.UpdateStamp();
                return true;
            }

            if (Tools.Generic.LongerAgoThan(state.Stamp, timeout))
                TryKillDueNotRespondingTimeout(process, timeout);
            return false;
        }

        void TryKillDueNotRespondingTimeout(Process process, TimeSpan timeout) {
            try {
                process.Kill();
            } finally {
                _monitorKilled.OnNext(Tuple.Create(process.StartInfo, process.Id, timeout, "Responding"));
            }
        }

        static bool TryCheckResponding(Process process) {
            var responding = false;
            try {
                responding = process.Responding;
            } catch {}
            return responding;
        }

        public class ProcessState
        {
            public ProcessState() {
                Stamp = Tools.Generic.GetCurrentUtcDateTime;
                StreamLengths = new Tuple<long, long>(0, 0);
            }

            public Tuple<long, long> StreamLengths { get; set; }
            public DateTime Stamp { get; private set; }

            public void UpdateStamp() {
                Stamp = Tools.Generic.GetCurrentUtcDateTime;
            }
        }

        #endregion

        #region Async variants

        public async Task<ProcessExitResult> LaunchAsync(BasicLaunchInfo info) {
            using (var process = new ReactiveProcess {StartInfo = info.StartInfo}) {
                return
                    await
                        LaunchAndWaitForExitAsync(process, info.MonitorOutput, info.MonitorResponding,
                            info.CancellationToken)
                            .ConfigureAwait(false);
            }
        }

        public async Task<ProcessExitResult> LaunchAndProcessAsync(LaunchAndProcessInfo info) {
            using (var process = new ReactiveProcess {StartInfo = info.StartInfo.EnableRedirect()}) {
                SetupStandardOutput(info, process);
                SetupStandardError(info, process);
                return
                    await
                        LaunchAndWaitForExitAsync(process, info.MonitorOutput, info.MonitorResponding,
                            info.CancellationToken)
                            .ConfigureAwait(false);
            }
        }

        static void SetupStandardError(LaunchAndProcessInfo info, ReactiveProcess process) {
            if (info.StandardErrorAction == null)
                return;
            if (!info.StartInfo.RedirectStandardError)
                throw new InvalidOperationException("Not redirected error");
            process.StandardErrorObservable.Subscribe(data => info.StandardErrorAction(process, data));
        }

        static void SetupStandardOutput(LaunchAndProcessInfo info, ReactiveProcess process) {
            if (info.StandardOutputAction == null)
                return;
            if (!info.StartInfo.RedirectStandardOutput)
                throw new InvalidOperationException("Not redirected output");
            process.StandardOutputObservable.Subscribe(data => info.StandardOutputAction(process, data));
        }

        public async Task<ProcessExitResultWithOutput> LaunchAndGrabAsync(BasicLaunchInfo info) {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var outInfo =
                await
                    LaunchAndProcessAsync(new LaunchAndProcessInfo(info.StartInfo.EnableRedirect()) {
                        StandardOutputAction = (process, data) => outputBuilder.AppendLine(data),
                        StandardErrorAction = (process, data) => errorBuilder.AppendLine(data),
                        MonitorOutput = info.MonitorOutput,
                        MonitorResponding = info.MonitorResponding
                    }).ConfigureAwait(false);
            return new ProcessExitResultWithOutput(outInfo.ExitCode, outInfo.Id, info.StartInfo,
                outputBuilder.ToString(),
                errorBuilder.ToString());
        }

        async Task<ProcessExitResult> LaunchAndWaitForExitAsync(ReactiveProcess process, TimeSpan? monitorOutput,
            TimeSpan? monitorResponding) {
            var task = process.StartAsync();
            _launched.OnNext(Tuple.Create(process.StartInfo, process.Id));

            using (SetupMonitoringDisposable(process, monitorOutput, monitorResponding))
                await task.ConfigureAwait(false);
            _terminated.OnNext(Tuple.Create(process.StartInfo, process.ExitCode, process.Id));
            return new ProcessExitResult(process.ExitCode, process.Id, process.StartInfo);
        }

        async Task<ProcessExitResult> LaunchAndWaitForExitAsync(ReactiveProcess process, TimeSpan? monitorOutput,
            TimeSpan? monitorResponding, CancellationToken token) {
            var task = process.StartAsync();
            _launched.OnNext(Tuple.Create(process.StartInfo, process.Id));

            using (SetupMonitoringDisposable(process, monitorOutput, monitorResponding))
            using (token.Register(process.Kill))
                await task.ConfigureAwait(false);
            _terminated.OnNext(Tuple.Create(process.StartInfo, process.ExitCode, process.Id));
            return new ProcessExitResult(process.ExitCode, process.Id, process.StartInfo);
        }

        #endregion

        #region Sync Variants

        public Process Start(ProcessStartInfo startInfo) {
            startInfo.Validate();
            var process = Process.Start(startInfo);
            _launched.OnNext(Tuple.Create(startInfo, process == null ? -1 : process.Id));
            return process;
        }

        public void StartAndForget(ProcessStartInfo startInfo) {
            using (Start(startInfo)) {}
        }

        public ProcessExitResult Launch(BasicLaunchInfo info) {
            using (var process = new Process {StartInfo = info.StartInfo}) {
                LaunchAndWaitForExit(process, info.MonitorOutput, info.MonitorResponding);
                return new ProcessExitResult(process.ExitCode, process.Id, info.StartInfo);
            }
        }

        public ProcessExitResultWithOutput LaunchAndGrab(BasicLaunchInfo info) {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var outInfo = LaunchAndProcess(new LaunchAndProcessInfo(info.StartInfo.EnableRedirect()) {
                StandardOutputAction = (process, data) => outputBuilder.AppendLine(data),
                StandardErrorAction = (process, data) => errorBuilder.AppendLine(data),
                MonitorOutput = info.MonitorOutput,
                MonitorResponding = info.MonitorResponding
            });
            return new ProcessExitResultWithOutput(outInfo.ExitCode, outInfo.Id, info.StartInfo,
                outputBuilder.ToString(),
                errorBuilder.ToString());
        }

        public virtual ProcessExitResultWithOutput LaunchAndGrabTool(ProcessStartInfo startInfo, string tool = null) {
            var ret = LaunchAndGrab(new BasicLaunchInfo(startInfo));
            if (ret.ExitCode == 0)
                return ret;

            throw new ProcessException(
                String.Format("{6} [{7}] error: {0} while running: {1} {2} from {3}\nOutput: {4}\nError: {5}",
                    ret.ExitCode,
                    startInfo.FileName,
                    startInfo.Arguments,
                    startInfo.WorkingDirectory,
                    ret.StandardOutput,
                    ret.StandardError,
                    tool ?? Path.GetFileNameWithoutExtension(startInfo.FileName),
                    ret.Id));
        }

        public virtual ProcessExitResultWithOutput LaunchAndGrabToolCmd(ProcessStartInfo info,
            string tool) {
            var startInfo =
                new ProcessStartInfoBuilder(Common.Paths.CmdExe,
                    String.Format("/C \"\"{0}\" {1}\"", info.FileName, info.Arguments)) {
                        WorkingDirectory = info.WorkingDirectory
                    }.Build();
            return LaunchAndGrabTool(startInfo, tool);
        }

        public ProcessExitResult LaunchAndProcess(LaunchAndProcessInfo info) {
            return LaunchAndProcessAsync(info).WaitAndUnwrapException();
        }

        void LaunchAndWaitForExit(Process process, TimeSpan? monitorOutput = null, TimeSpan? monitorResponding = null) {
            var waitHandler = new WaitForExitHandler(process);
            using (StartProcess(process, monitorOutput, monitorResponding))
                waitHandler.WaitForExit();
            _terminated.OnNext(Tuple.Create(process.StartInfo, process.ExitCode, process.Id));
        }

        class AsyncWaitForExitHandler : IDisposable
        {
            Process _process;
            Task _task;
            TaskCompletionSource<object> _tcs;
            TaskCompletionSource<object> _tcsError;
            TaskCompletionSource<object> _tcsOutput;

            public AsyncWaitForExitHandler(Process process,
                CancellationToken cancellationToken = default(CancellationToken)) {
                _process = process;
                _tcs = new TaskCompletionSource<object>();

                // Best to attach these handlers last because that should mean we have processed all data
                // Unclear however what happens if there is an error in any of the earlier handlers...
                if (process.StartInfo.RedirectStandardOutput)
                    SetupOutput();
                if (process.StartInfo.RedirectStandardError)
                    SetupError();

                var tcs = new[] {_tcs, _tcsError, _tcsOutput}.Where(x => x != null).ToArray();
                if (cancellationToken != default(CancellationToken)) {
                    foreach (var t in tcs)
                        cancellationToken.Register(() => t.TrySetCanceled());
                }

                process.EnableRaisingEvents = true;
                process.Exited += Exited;
                _task = Task.WhenAll(tcs.Select(x => x.Task));
            }

            public void Dispose() {
                _process.OutputDataReceived -= OnProcessOnOutputDataReceived;
                _process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
                _process.Exited -= Exited;
                _process.EnableRaisingEvents = false;
                _process = null;
                _tcsOutput = null;
                _tcsError = null;
                _tcs = null;
                _task = null;
            }

            public Task WaitForExit() {
                if (_process.StartInfo.RedirectStandardOutput)
                    _process.BeginOutputReadLine();
                if (_process.StartInfo.RedirectStandardError)
                    _process.BeginErrorReadLine();
                return _task;
            }

            void SetupOutput() {
                _tcsOutput = new TaskCompletionSource<object>();
                _process.OutputDataReceived += OnProcessOnOutputDataReceived;
            }

            void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e) {
                if (e.Data != null)
                    return;
                _tcsOutput.TrySetResult(null);
            }

            void SetupError() {
                _tcsError = new TaskCompletionSource<object>();
                _process.ErrorDataReceived += OnProcessOnErrorDataReceived;
            }

            void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e) {
                if (e.Data != null)
                    return;
                _tcsError.TrySetResult(null);
            }

            void Exited(object sender, EventArgs eventArgs) {
                // For some reason had to use TrySetResult instead of SetResult, because otherwise it might throw 'already ran to completion' error...
                _tcs.TrySetResult(null);
            }
        }

        class WaitForExitHandler
        {
            readonly Process _process;
            AutoResetEvent _errorWaitHandle;
            AutoResetEvent _outputWaitHandle;

            public WaitForExitHandler(Process process) {
                _process = process;
                // Best to attach these handlers last because that should mean we have processed all data
                // Unclear however what happens if there is an error in any of the earlier handlers...
                if (_process.StartInfo.RedirectStandardOutput)
                    process.OutputDataReceived += OnProcessOnOutputDataReceived;

                if (_process.StartInfo.RedirectStandardError)
                    process.ErrorDataReceived += OnProcessOnErrorDataReceived;
            }

            void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args) {
                if (args.Data != null)
                    return;
                _process.OutputDataReceived -= OnProcessOnOutputDataReceived;
                _outputWaitHandle.Set();
            }

            void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs args) {
                if (args.Data != null)
                    return;
                _process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
                _errorWaitHandle.Set();
            }

            public void WaitForExit() {
                using (_outputWaitHandle = _process.StartInfo.RedirectStandardOutput ? new AutoResetEvent(false) : null)
                using (
                    _errorWaitHandle = _process.StartInfo.RedirectStandardError ? new AutoResetEvent(false) : null) {
                    if (_process.StartInfo.RedirectStandardOutput)
                        _process.BeginOutputReadLine();
                    if (_process.StartInfo.RedirectStandardError)
                        _process.BeginErrorReadLine();
                    _process.WaitForExit();
                    if (_outputWaitHandle != null)
                        _outputWaitHandle.WaitOne();
                    if (_errorWaitHandle != null)
                        _errorWaitHandle.WaitOne();
                }
            }
        }

        #endregion
    }
}