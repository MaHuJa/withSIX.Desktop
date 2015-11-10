// <copyright company="SIX Networks GmbH" file="FileCopier.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public interface ICopyFile
    {
        void CopyFile(IAbsoluteFilePath a, IAbsoluteFilePath b);
        Task CopyFileAsync(IAbsoluteFilePath a, IAbsoluteFilePath b);
    }

    public class FileCopier : ICopyFile
    {
        readonly Tools.FileTools.IFileOps _fileOps;

        public FileCopier(Tools.FileTools.IFileOps fileOps) {
            _fileOps = fileOps;
        }

        public void CopyFile(IAbsoluteFilePath a, IAbsoluteFilePath b) {
            _fileOps.CopyWithRetry(a, b);
        }

        public Task CopyFileAsync(IAbsoluteFilePath a, IAbsoluteFilePath b) {
            return _fileOps.CopyAsyncWithRetry(a, b);
        }
    }
}