// <copyright company="SIX Networks GmbH" file="CompressionTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Tests.Core.Integration
{
    [TestFixture, Ignore, Category("Integration")]
    public class CompressionTest
    {
        [Test]
        public void UnpackGzWithRar() {
            var path = Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
                "Resources");
            var f = Path.Combine(path, "f4_psd.rar.gz");

            Tools.Compression.Unpack(f.ToAbsoluteFilePath(), @"C:\temp".ToAbsoluteDirectoryPath());
        }
    }
}