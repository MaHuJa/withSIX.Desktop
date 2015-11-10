// <copyright company="SIX Networks GmbH" file="MultiMirrorFileDownloadSpec.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading;
using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Sync.Core.Transfer.Specs
{
    public class MultiMirrorFileDownloadSpec
    {
        public readonly IAbsoluteFilePath LocalFile;
        public readonly string RemoteFile;

        public MultiMirrorFileDownloadSpec(string remoteFile, IAbsoluteFilePath localFile) {
            Contract.Requires<ArgumentNullException>(remoteFile != null);
            Contract.Requires<ArgumentNullException>(localFile != null);
            RemoteFile = remoteFile;
            LocalFile = localFile;
        }

        public MultiMirrorFileDownloadSpec(string remoteFile, IAbsoluteFilePath localFile,
            Func<IAbsoluteFilePath, bool> confirmValidity)
            : this(remoteFile, localFile) {
            Contract.Requires<ArgumentNullException>(confirmValidity != null);
            Verification = confirmValidity;
        }

        public ITransferStatus Progress { get; set; }
        public Func<IAbsoluteFilePath, bool> Verification { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public Uri GetUri(Uri host) {
            return Tools.Transfer.JoinUri(host, Tools.Transfer.EncodePathIfRequired(host, RemoteFile));
        }

        public void Start(RepoStatus action = RepoStatus.Downloading) {
            if (Progress != null) {
                Progress.StartOutput(LocalFile.ToString());
                Progress.Action = action;
            }
        }

        public void End() {
            if (Progress != null)
                Progress.EndOutput(LocalFile.ToString());
        }

        public void Fail() {
            if (Progress != null)
                Progress.FailOutput(LocalFile.ToString());
        }

        public void UpdateHost(Uri host) {
            if (Progress != null)
                Progress.Info = host.ToString();
        }
    }
}