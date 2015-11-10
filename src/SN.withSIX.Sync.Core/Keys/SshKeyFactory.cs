// <copyright company="SIX Networks GmbH" file="SshKeyFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.Keys
{
    public class SshKeyFactory : IDomainService
    {
        static readonly IAbsoluteFilePath sshKeyGenBin =
            Common.Paths.ToolMinGwBinPath.GetChildFileWithName("ssh-keygen.exe");
        readonly IProcessManager _processManager;

        public SshKeyFactory(IProcessManager processManager) {
            _processManager = processManager;
        }

        public SshKeyPair Create(string outFile, bool overwrite = false, int bits = SshKeyPair.DefaultBits,
            string type = SshKeyPair.DefaultType) {
            Contract.Requires<ArgumentNullException>(outFile != null);
            CreateFiles(outFile, overwrite, bits, type);
            return new SshKeyPair(outFile);
        }

        void CreateFiles(string outFile, bool overwrite = false, int bits = SshKeyPair.DefaultBits,
            string type = SshKeyPair.DefaultType) {
            if (!overwrite && File.Exists(outFile))
                throw new IOException("File exists " + outFile);
            var startInfo = new ProcessStartInfoBuilder(sshKeyGenBin,
                GetParameters(outFile, bits, type)) {
                    WorkingDirectory = Common.Paths.AppPath.ToString()
                }.Build();
            _processManager.LaunchAndGrabTool(startInfo);
        }

        static string GetParameters(string outFile, int bits, string type) {
            return String.Format("-q -P \"\" -b {1} -t {2} -f \"{0}\"", outFile, bits, type);
        }
    }
}