// <copyright company="SIX Networks GmbH" file="RsyncController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SN.withSIX.Core;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class RsyncController : IRsyncController
    {
        readonly IRsyncLauncher _rsyncLauncher;

        public RsyncController(string local, string remote, string key, IRsyncLauncher rsyncLauncher) {
            if (rsyncLauncher == null)
                throw new ArgumentNullException("rsyncLauncher");

            Local = local;
            Remote = remote;
            Key = key;
            _rsyncLauncher = rsyncLauncher;
        }

        protected string Local { get; set; }
        protected string Remote { get; set; }
        protected string Key { get; set; }

        public void Push(string localSub = null, string remoteSub = null) {
            CreateSshFolder();
            HandleRsyncResponse(_rsyncLauncher.Run(JoinPathsIfNeeded(Local, localSub),
                JoinPathsIfNeeded(Remote, remoteSub),
                BuildOptions()));
        }

        public void Push(ITransferProgress status, string localSub = null, string remoteSub = null) {
            CreateSshFolder();
            HandleRsyncResponse(_rsyncLauncher.RunAndProcess(
                status,
                JoinPathsIfNeeded(Local, localSub),
                JoinPathsIfNeeded(Remote, remoteSub),
                BuildOptions()));
        }

        private RsyncOptions BuildOptions() {
            return new RsyncOptions { Key = Key};
        }

        public void Pull(string remoteSub = null, string localSub = null) {
            CreateSshFolder();
            HandleRsyncResponse(_rsyncLauncher.Run(JoinPathsIfNeeded(Remote, remoteSub),
                JoinPathsIfNeeded(Local, localSub),
                BuildOptions()));
        }

        public void Pull(ITransferProgress status, string remoteSub = null, string localSub = null) {
            CreateSshFolder();
            HandleRsyncResponse(_rsyncLauncher.RunAndProcess(
                status,
                JoinPathsIfNeeded(Remote, remoteSub),
                JoinPathsIfNeeded(Local, localSub),
                BuildOptions()));
        }

        void CreateSshFolder() {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh").MakeSurePathExists();
        }

        protected void HandleRsyncResponse(ProcessExitResultWithOutput response) {
            if (response.ExitCode == 0)
                return;
            throw new RsyncException(string.Format("Rsync [{0}] exited with code {1}\nOutput: {2}\nError: {3}",
                response.Id, response.ExitCode, response.StandardOutput, response.StandardError));
        }

        protected void HandleRsyncResponse(ProcessExitResult response) {
            if (response.ExitCode == 0)
                return;
            throw new RsyncException(string.Format("Rsync [{0}] exited with code {1}",
                response.Id, response.ExitCode));
        }

        protected static string JoinPathsIfNeeded(string path1, string path2) {
            return path2 == null ? path1 : Path.Combine(path1, path2);
        }
    }
}