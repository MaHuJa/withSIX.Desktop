// <copyright company="SIX Networks GmbH" file="TransferSpec.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.Transfer.Specs
{
    public abstract class TransferSpec
    {
        protected TransferSpec(Uri uri, IAbsoluteFilePath localFile, ITransferProgress progress) {
            Contract.Requires<ArgumentNullException>(uri != null);
            Contract.Requires<ArgumentNullException>(localFile != null);

            Uri = uri;
            LocalFile = localFile;
            Progress = progress ?? new TransferProgress();
        }

        public CancellationToken CancellationToken { get; set; }
        public IAbsoluteFilePath LocalFile { get; private set; }
        public ITransferProgress Progress { get; private set; }
        public Uri Uri { get; private set; }
        public Func<IAbsoluteFilePath, bool> Verification { get; set; }
    }
}