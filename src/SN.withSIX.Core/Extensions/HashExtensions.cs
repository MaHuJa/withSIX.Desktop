// <copyright company="SIX Networks GmbH" file="HashExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NDepend.Path;

namespace SN.withSIX.Core.Extensions
{
    public static class Hash
    {
        public static string Sha256String(this string password, int rotations = 1) {
            return BitConverter.ToString(Sha256(password, rotations)).Replace("-", string.Empty);
        }

        public static byte[] Sha256(this string password, int rotations = 1) {
            using (var sha256CryptoServiceProvider = new SHA256CryptoServiceProvider()) {
                var hash = Encoding.UTF8.GetBytes(password);

                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = sha256CryptoServiceProvider.ComputeHash(hash);

                return hash;
            }
        }

        public static string Sha1String(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            return BitConverter.ToString(Sha1(password, rotations)).Replace("-", string.Empty);
        }

        public static string Sha1String(this FileStream stream, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            return BitConverter.ToString(Sha1(stream, rotations)).Replace("-", string.Empty);
        }

        public static byte[] Sha1(this FileStream stream, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            using (var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider()) {
                byte[] hash = null;
                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = sha1CryptoServiceProvider.ComputeHash(stream);
                return hash;
            }
        }

        public static byte[] Sha1(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            using (var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider()) {
                var hash = Encoding.UTF8.GetBytes(password);

                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = sha1CryptoServiceProvider.ComputeHash(hash);

                return hash;
            }
        }

        public static byte[] Sha1(this byte[] hash, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            using (var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider()) {
                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = sha1CryptoServiceProvider.ComputeHash(hash);
                return hash;
            }
        }

        public static string MD5String(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            return BitConverter.ToString(MD5(password, rotations)).Replace("-", string.Empty);
        }

        public static string GetMd5HasFromStream(Stream stream) {
            using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider())
                return BitConverter.ToString(md5CryptoServiceProvider.ComputeHash(stream)).Replace("-", string.Empty);
        }

        public static string GetMd5HashFromFile(IAbsoluteFilePath path) {
            using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider())
            using (var stream = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read))
                return BitConverter.ToString(md5CryptoServiceProvider.ComputeHash(stream)).Replace("-", string.Empty);
        }

        public static byte[] MD5(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider()) {
                var hash = Encoding.UTF8.GetBytes(password);

                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = md5CryptoServiceProvider.ComputeHash(hash);

                return hash;
            }
        }

        public static string Sha512String(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            for (var iterator = 0; iterator < rotations; iterator++) {
                password = BitConverter.ToString(Sha512(password, 1))
                    .Replace("-", string.Empty)
                    .ToLower();
            }

            return password;
        }

        public static byte[] Sha512(this string password, int rotations = 1) {
            Contract.Requires<ArgumentNullException>(rotations > 0);
            using (var sha512CryptoServiceProvider = new SHA512CryptoServiceProvider()) {
                var hash = Encoding.UTF8.GetBytes(password);

                for (byte iterator = 0; iterator < rotations; ++iterator)
                    hash = sha512CryptoServiceProvider.ComputeHash(hash);

                return hash;
            }
        }
    }
}