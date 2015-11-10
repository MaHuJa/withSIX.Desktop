// <copyright company="SIX Networks GmbH" file="HashEncryption.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NDepend.Path;

namespace SN.withSIX.Core
{
    public static partial class Tools
    {
        public static HashEncryptionTools HashEncryption = new HashEncryptionTools();

        public class HashEncryptionTools
        {
            public string MD5Hash(string data) {
                Contract.Requires<ArgumentNullException>(data != null);

                using (MD5 md5 = new MD5CryptoServiceProvider())
                    return GetHash(md5.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }

            string GetHash(byte[] hash) {
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }

            public string MD5FileHash(IAbsoluteFilePath fileName) {
                Contract.Requires<ArgumentNullException>(fileName != null);

                return FileUtil.Ops.AddIORetryDialog(() => {
                    using (var fs = new FileStream(fileName.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var stream = new BufferedStream(fs, GetBufferSize(fs)))
                    using (MD5 md5 = new MD5CryptoServiceProvider())
                        return GetHash(md5.ComputeHash(stream));
                }, fileName.ToString());
            }

            public string SHA1FileHash(IAbsoluteFilePath fileName) {
                Contract.Requires<ArgumentNullException>(fileName != null);

                return FileUtil.Ops.AddIORetryDialog(() => {
                    using (var fs = new FileStream(fileName.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var stream = new BufferedStream(fs, GetBufferSize(fs)))
                    using (var md5 = new SHA1CryptoServiceProvider())
                        return GetHash(md5.ComputeHash(stream));
                }, fileName.ToString());
            }

            public int GetBufferSize(FileStream fs) {
                var buffer = 1200000 > fs.Length ? (int) fs.Length : 1200000;
                return buffer > 0 ? buffer : 1;
            }

            public string SHA256FileHash(string fileName) {
                Contract.Requires<ArgumentNullException>(fileName != null);
                Contract.Requires<ArgumentOutOfRangeException>(!String.IsNullOrEmpty(fileName));

                return FileUtil.Ops.AddIORetryDialog(() => {
                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var stream = new BufferedStream(fs, GetBufferSize(fs)))
                    using (var md5 = new SHA256CryptoServiceProvider())
                        return GetHash(md5.ComputeHash(stream));
                }, fileName);
            }

            public string SHA256Hash(string data) {
                Contract.Requires<ArgumentNullException>(data != null);

                using (SHA256 sha = new SHA256CryptoServiceProvider())
                    return GetHash(sha.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }

            public string SHA384Hash(string data) {
                Contract.Requires<ArgumentNullException>(data != null);

                using (SHA384 sha = new SHA384CryptoServiceProvider())
                    return GetHash(sha.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }

            public string SHA512Hash(string data) {
                Contract.Requires<ArgumentNullException>(data != null);

                using (SHA512 sha = new SHA512CryptoServiceProvider())
                    return GetHash(sha.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }
        }
    }
}