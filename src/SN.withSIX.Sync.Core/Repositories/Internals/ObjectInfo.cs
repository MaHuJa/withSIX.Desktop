// <copyright company="SIX Networks GmbH" file="ObjectInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    public class ObjectInfo
    {
        public ObjectInfo() {}

        public ObjectInfo(string checksum, string checksumPack) {
            Checksum = checksum;
            ChecksumPack = checksumPack;
        }

        public string Checksum { get; protected set; }
        public string ChecksumPack { get; protected set; }
    }
}