// <copyright company="SIX Networks GmbH" file="IFileWriter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;

namespace SN.withSIX.Core.Services.Infrastructure
{
    [ContractClass(typeof (FileWriterContract))]
    public interface IFileWriter
    {
        void WriteFile(string fileName, string fileContent);
        Task WriteFileAsync(string fileName, string fileContent);
        void WriteFile(string fileName, string fileContent, Encoding encoding);
        Task WriteFileAsync(string fileName, string fileContent, Encoding encoding);
    }

    [ContractClassFor(typeof (IFileWriter))]
    public abstract class FileWriterContract : IFileWriter
    {
        public void WriteFile(string fileName, string fileContent) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires<ArgumentNullException>(fileContent != null);
        }

        public Task WriteFileAsync(string fileName, string fileContent) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires<ArgumentNullException>(fileContent != null);
            return default(Task);
        }

        public void WriteFile(string fileName, string fileContent, Encoding encoding) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires<ArgumentNullException>(fileContent != null);
        }

        public Task WriteFileAsync(string fileName, string fileContent, Encoding encoding) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires<ArgumentNullException>(fileContent != null);
            return default(Task);
        }
    }
}