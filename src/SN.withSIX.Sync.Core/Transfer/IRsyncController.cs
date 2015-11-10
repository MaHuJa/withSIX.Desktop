// <copyright company="SIX Networks GmbH" file="IRsyncController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Sync.Core.Transfer
{
    public interface IRsyncController
    {
        void Push(string localSub = null, string remoteSub = null);
        void Push(ITransferProgress status, string localSub = null, string remoteSub = null);
        void Pull(string remoteSub = null, string localSub = null);
        void Pull(ITransferProgress status, string remoteSub = null, string localSub = null);
    }
}