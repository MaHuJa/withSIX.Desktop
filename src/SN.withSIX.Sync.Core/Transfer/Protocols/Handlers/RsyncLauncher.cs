// <copyright company="SIX Networks GmbH" file="RsyncLauncher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public class RsyncLauncher : IRsyncLauncher
    {
        protected const int DEFAULT_TIMEOUT = 100;
        const bool UseCygwinRsync = true;
        static readonly string sshKeyParams = "-o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no";
        public static readonly string DEFAULT_RSYNC_PARAMS =
            String.Format("--times -O --no-whole-file -r --delete --partial --progress -h --timeout={0}",
                DEFAULT_TIMEOUT);
        static readonly string defaultParams = DEFAULT_RSYNC_PARAMS;
        readonly IAbsoluteFilePath _binPath;
        readonly RsyncOutputParser _parser;
        readonly IProcessManager _processManager;
        readonly IAbsoluteFilePath _sshBinPath;

        public RsyncLauncher(IProcessManager processManager, IPathConfiguration configuration,
            RsyncOutputParser parser) {
            if (processManager == null)
                throw new ArgumentNullException("processManager");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _processManager = processManager;
            _parser = parser;
            _binPath = configuration.ToolCygwinBinPath.GetChildFileWithName("rsync.exe");
            _sshBinPath = configuration.ToolCygwinBinPath.GetChildFileWithName("ssh.exe");
        }

        public ProcessExitResultWithOutput Run(string source, string destination, string key = null) {
            var startInfo = new ProcessStartInfo(_binPath.ToString(), JoinArgs(source, destination, key))
                .SetWorkingDirectoryOrDefault(Directory.GetCurrentDirectory());

            return
                _processManager.LaunchAndGrab(
                    new BasicLaunchInfo(
                        startInfo) {
                            MonitorOutput = _processManager.DefaultMonitorOutputTimeOut,
                            MonitorResponding = _processManager.DefaultMonitorRespondingTimeOut
                        });
        }

        public ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, string source, string destination,
            string key = null) {
            var processInfo = BuildProcessInfo(progress, source, destination, key);
            return ProcessExitResultWithOutput.FromProcessExitResult(_processManager.LaunchAndProcess(processInfo),
                progress.Output);
        }

        public async Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, string source,
            string destination,
            string key = null) {
            var processInfo = BuildProcessInfo(progress, source, destination, key);
            return
                ProcessExitResultWithOutput.FromProcessExitResult(
                    await _processManager.LaunchAndProcessAsync(processInfo).ConfigureAwait(false), progress.Output);
        }

        public ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, string source, string destination,
            CancellationToken token,
            string key = null) {
            var processInfo = BuildProcessInfo(progress, source, destination, key);
            processInfo.CancellationToken = token;
            return ProcessExitResultWithOutput.FromProcessExitResult(_processManager.LaunchAndProcess(processInfo),
                progress.Output);
        }

        public async Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, string source,
            string destination, CancellationToken token,
            string key = null) {
            var processInfo = BuildProcessInfo(progress, source, destination, key);
            processInfo.CancellationToken = token;
            return
                ProcessExitResultWithOutput.FromProcessExitResult(
                    await _processManager.LaunchAndProcessAsync(processInfo).ConfigureAwait(false), progress.Output);
        }

        LaunchAndProcessInfo BuildProcessInfo(ITransferProgress progress, string source, string destination, string key) {
            return new LaunchAndProcessInfo(GetProcessStartInfo(source, destination, key)) {
                StandardOutputAction = (process, data) => _parser.ParseOutput(process, data, progress),
                StandardErrorAction = (process, data) => _parser.ParseOutput(process, data, progress),
                MonitorOutput = _processManager.DefaultMonitorOutputTimeOut,
                MonitorResponding = _processManager.DefaultMonitorRespondingTimeOut
            };
        }

        ProcessStartInfo GetProcessStartInfo(string source, string destination, string key) {
            return new ProcessStartInfoBuilder(_binPath, JoinArgs(source, destination, key)) {
                WorkingDirectory = Directory.GetCurrentDirectory()
            }.Build();
        }

        IEnumerable<string> GetArguments(string source, string destination, string key) {
            var args = new[] {defaultParams}.ToList();
            if (key != null)
                args.Add(string.Format("-e \"'{0}' {1} -i '{2}'\"", _sshBinPath, sshKeyParams, HandlePath(key)));
            args.Add(HandlePath(source).EscapePath());
            args.Add(HandlePath(destination).EscapePath());
            return args;
        }

        string JoinArgs(string source, string destination, string key) {
            return string.Join(" ", GetArguments(source, destination, key));
        }

        static string HandlePath(string path) {
#pragma warning disable 162
            return UseCygwinRsync
                ? path.CygwinPath()
                : path.MingwPath();
#pragma warning restore 162
        }
    }
}