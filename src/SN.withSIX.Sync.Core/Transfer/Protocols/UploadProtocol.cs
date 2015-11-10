// <copyright company="SIX Networks GmbH" file="UploadProtocol.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer.Protocols
{
    public interface IUploadProtocol : IProtocol
    {
        void Upload(TransferSpec spec);
        Task UploadAsync(TransferSpec spec);
    }

    public interface IProtocol
    {
        IEnumerable<string> Schemes { get; }
    }

    public abstract class UploadProtocol : TransferProtocol, IUploadProtocol
    {
        public abstract void Upload(TransferSpec spec);
        public abstract Task UploadAsync(TransferSpec spec);
    }
}