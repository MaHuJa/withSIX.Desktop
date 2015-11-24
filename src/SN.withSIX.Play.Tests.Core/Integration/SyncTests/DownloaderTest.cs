// <copyright company="SIX Networks GmbH" file="DownloaderTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Data.Services;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;

namespace SN.withSIX.Play.Tests.Core.Integration.SyncTests
{
    [TestFixture, Ignore(""), Category("Integration")]
    public class DownloaderTest
    {
        [SetUp]
        public void SetUp() {
            if (File.Exists(TestFile))
                File.Delete(TestFile);
        }

        const string TestFile = @"C:\temp\testFile.txt";

        static void DownloadWithStrategy(string url, IDownloadProtocol strategy) {
            new FileDownloader(new[] {strategy}, A.Fake<IAuthProvider>()).Download(url, TestFile.ToAbsoluteFilePath());
        }

        static void SupportInit() {
            SharedSupport.Init();
            SharedSupport.HandleTools();
        }

        [Test, Category("Slow"), Category("Integration")]
        public void DownloadFileFromHttp() {
            var webClientFactory = SharedSupport.CreateWebClientExportFactory();

            DownloadWithStrategy("http://androidnetworktester.googlecode.com/files/1mb.txt",
                new HttpDownloadProtocol(webClientFactory, A.Fake<Tools.FileTools.IFileOps>()));

            File.Exists(TestFile).Should().BeTrue("because the file should be downloaded");
        }

        [Test, Category("Slow"), Category("Integration")]
        public void DownloadFileFromRsync() {
            SupportInit();

            DownloadWithStrategy("rsync://c1-de.sixmirror.com/su/rel/cba/.pack/.repository.yml",
                new RsyncDownloadProtocol(
                    new RsyncLauncher(new ProcessManager(),
                        Common.Paths, new RsyncOutputParser())));

            File.Exists(TestFile).Should().BeTrue("because the file should be downloaded");
        }

        [Test, Category("Slow"), Category("Integration")]
        public void DownloadFileFromZsync() {
            SupportInit();

            DownloadWithStrategy("zsync://c1-de.sixmirror.com/rel/cba/.pack/.repository.yml.zsync",
                new ZsyncDownloadProtocol(
                    new ZsyncLauncher(new ProcessManager(),
                        Common.Paths, new ZsyncOutputParser(), new AuthProvider(new AuthProviderSettingsStorage(new UserSettings())))));

            File.Exists(TestFile).Should().BeTrue("because the file should be downloaded");
        }
    }
}