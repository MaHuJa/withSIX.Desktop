// <copyright company="SIX Networks GmbH" file="DownloadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public interface IDownloadProtocol : IProtocol
    {
        void Download(TransferSpec spec);
        Task DownloadAsync(TransferSpec spec);
    }

    public abstract class DownloadProtocol : TransferProtocol, IDownloadProtocol
    {
        public abstract void Download(TransferSpec spec);
        public abstract Task DownloadAsync(TransferSpec spec);

        protected static void VerifyIfNeeded(TransferSpec spec, IAbsoluteFilePath localFile) {
            if (spec.Verification == null || spec.Verification(localFile))
                return;
            Tools.FileUtil.Ops.DeleteFile(localFile);
            throw new VerificationError(localFile.ToString());
        }
    }
}