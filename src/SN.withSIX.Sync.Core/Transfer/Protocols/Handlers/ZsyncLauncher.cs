// <copyright company="SIX Networks GmbH" file="ZsyncLauncher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public interface IZsyncLauncher
    {
        ProcessExitResultWithOutput Run(Uri uri, string file);
        ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, Uri uri, string file);

        ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, Uri uri, string file,
            CancellationToken token);

        Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, Uri url, string file);

        Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, Uri url, string file,
            CancellationToken token);
    }

    public class ZsyncLauncher : IZsyncLauncher
    {
        const bool UseCygwinZsync = true;
        static readonly string[] tempExtensions = {".zs-old", Tools.GenericTools.TmpExtension, ".part"};
        readonly IAuthProvider _authProvider;
        readonly IAbsoluteFilePath _binPath;
        readonly ZsyncOutputParser _parser;
        readonly IProcessManager _processManager;

        public ZsyncLauncher(IProcessManager processManager, IPathConfiguration configuration,
            ZsyncOutputParser parser, IAuthProvider authProvider) {
            if (processManager == null)
                throw new ArgumentNullException("processManager");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _authProvider = authProvider;

            _processManager = processManager;
            _parser = parser;
            _binPath = configuration.ToolCygwinBinPath.GetChildFileWithName("zsync.exe");
        }

        public ProcessExitResultWithOutput Run(Uri uri, string file) {
            TryHandleOldFiles(file);
            var startInfo = new ProcessStartInfo(_binPath.ToString(), GetArgs(uri, file))
                .SetWorkingDirectoryOrDefault(GetDirectory(file));
            var r = _processManager.LaunchAndGrab(new BasicLaunchInfo(startInfo));
            TryRemoveOldFiles(file);
            return r;
        }

        public ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, Uri url, string file) {
            TryHandleOldFiles(file);
            var processInfo = BuildProcessInfo(progress, url, file);
            var r =
                ProcessExitResultWithOutput.FromProcessExitResult(_processManager.LaunchAndProcess(processInfo),
                    progress.Output);
            TryRemoveOldFiles(file);
            return r;
        }

        public ProcessExitResultWithOutput RunAndProcess(ITransferProgress progress, Uri url, string file,
            CancellationToken token) {
            TryHandleOldFiles(file);
            var processInfo = BuildProcessInfo(progress, url, file);
            var r =
                ProcessExitResultWithOutput.FromProcessExitResult(_processManager.LaunchAndProcess(processInfo),
                    progress.Output);
            TryRemoveOldFiles(file);
            return r;
        }

        public async Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, Uri url,
            string file) {
            TryHandleOldFiles(file);
            var processInfo = BuildProcessInfo(progress, url, file);
            var r =
                ProcessExitResultWithOutput.FromProcessExitResult(
                    await _processManager.LaunchAndProcessAsync(processInfo).ConfigureAwait(false),
                    progress.Output);
            TryRemoveOldFiles(file);
            return r;
        }

        public async Task<ProcessExitResultWithOutput> RunAndProcessAsync(ITransferProgress progress, Uri url,
            string file, CancellationToken token) {
            TryHandleOldFiles(file);
            var processInfo = BuildProcessInfo(progress, url, file);
            processInfo.CancellationToken = token;
            var r =
                ProcessExitResultWithOutput.FromProcessExitResult(
                    await _processManager.LaunchAndProcessAsync(processInfo).ConfigureAwait(false),
                    progress.Output);
            TryRemoveOldFiles(file);
            return r;
        }

        LaunchAndProcessInfo BuildProcessInfo(ITransferProgress progress, Uri url, string file) {
            return new LaunchAndProcessInfo(GetProcessStartInfo(url, file)) {
                StandardOutputAction = (x, args) => ParseOutput(x, args, progress),
                StandardErrorAction = (x, args) => ParseOutput(x, args, progress),
                MonitorOutput = _processManager.DefaultMonitorOutputTimeOut,
                MonitorResponding = _processManager.DefaultMonitorRespondingTimeOut
            };
        }

        static string GetDirectory(string file) {
            var path = Path.GetDirectoryName(file);
            return string.IsNullOrWhiteSpace(path)
                ? Directory.GetCurrentDirectory()
                : path;
        }

        static void TryHandleOldFiles(string localFile) {
            try {
                RemoveReadOnlyFromOldFiles(localFile);
            } catch (Exception e) {
                MainLog.Logger.FormattedDebugException(e);
            }
        }

        static void TryRemoveOldFiles(string localFile) {
            try {
                RemoveOldFiles(localFile);
            } catch (Exception e) {
                MainLog.Logger.FormattedDebugException(e);
            }
        }

        static void RemoveReadOnlyFromOldFiles(string localFile) {
            foreach (var file in tempExtensions.Select(x => localFile + x))
                file.RemoveReadonlyWhenExists();
        }

        static void RemoveOldFiles(string localFile) {
            foreach (var file in tempExtensions.Select(x => localFile + x))
                Tools.FileUtil.Ops.DeleteIfExists(file);
        }

        void ParseOutput(Process sender, string data, ITransferProgress progress) {
            _parser.ParseOutput(sender, data, progress);
        }

        ProcessStartInfo GetProcessStartInfo(Uri uri, string file) {
            return new ProcessStartInfoBuilder(_binPath, GetArgs(uri, file)) {
                WorkingDirectory = GetDirectory(file)
            }.Build();
        }

        string GetArgs(Uri uri, string file) {
            return !string.IsNullOrWhiteSpace(uri.UserInfo)
                ? GetArgsWithAuthInfo(uri, HandlePath(file))
                : GetArgsWithoutAuthInfo(uri, HandlePath(file));
        }

        static string GetArgsWithoutAuthInfo(Uri uri, string file) {
            return String.Format("{0}{1}-o \"{2}\" \"{3}\"", GetInputFile(file), GetDebugInfo(), file, uri.EscapedUri());
        }

        string GetArgsWithAuthInfo(Uri uri, string file) {
            return String.Format("{0}{1}{2}-o \"{3}\" \"{4}\"", GetInputFile(file), GetAuthInfo(uri), GetDebugInfo(),
                file,
                GetUri(uri).EscapedUri());
        }

        string GetAuthInfo(Uri uri) {
            var hostname = uri.Host;
            if (uri.Port != 80)
                hostname += (":" + uri.Port);

            var authInfo = _authProvider.GetAuthInfoFromUri(uri);
            return String.Format("-A {0}={1}:{2} ", hostname, authInfo.Username, authInfo.Password);
        }

        static string GetDebugInfo() {
            return Common.Flags.Verbose ? "-v " : null;
        }

        Uri GetUri(Uri uri) {
            return !string.IsNullOrWhiteSpace(uri.UserInfo) ? _authProvider.HandleUriAuth(uri) : uri;
        }

        static string GetInputFile(string file) {
            return File.Exists(file) ? String.Format("-i \"{0}\" ", file) : String.Empty;
        }

        static string HandlePath(string path) {
#pragma warning disable 162
            return UseCygwinZsync
                ? path.CygwinPath()
                : path.MingwPath();
#pragma warning restore 162
        }
    }
}