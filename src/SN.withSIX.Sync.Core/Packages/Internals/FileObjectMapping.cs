// <copyright company="SIX Networks GmbH" file="FileObjectMapping.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    public class FileObjectMapping
    {
        public FileObjectMapping(string filePath, string checksum) {
            FilePath = filePath;
            Checksum = checksum;
        }

        public string Checksum { get; protected set; }
        public string FilePath { get; protected set; }
    }
}