// <copyright company="SIX Networks GmbH" file="FileWriter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Text;
using System.Threading.Tasks;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Core.Infra.Services
{
    // TODO: IAbsoluteFilePath etc...
    class FileWriter : IFileWriter, IInfrastructureService
    {
        public void WriteFile(string fileName, string fileContent) {
            Path.GetDirectoryName(fileName).MakeSurePathExists();
            using (var sw = new StreamWriter(File.Open(fileName, FileMode.Create)))
                sw.Write(fileContent);
        }

        public async Task WriteFileAsync(string fileName, string fileContent) {
            Path.GetDirectoryName(fileName).MakeSurePathExists();
            using (var sw = new StreamWriter(File.Open(fileName, FileMode.Create)))
                await sw.WriteAsync(fileContent).ConfigureAwait(false);
        }

        public void WriteFile(string fileName, string fileContent, Encoding encoding) {
            Path.GetDirectoryName(fileName).MakeSurePathExists();
            using (var sw = new StreamWriter(File.Open(fileName, FileMode.Create), encoding))
                sw.Write(fileContent);
        }

        public async Task WriteFileAsync(string fileName, string fileContent, Encoding encoding) {
            Path.GetDirectoryName(fileName).MakeSurePathExists();
            using (var sw = new StreamWriter(File.Open(fileName, FileMode.Create), encoding))
                await sw.WriteAsync(fileContent).ConfigureAwait(false);
        }
    }
}