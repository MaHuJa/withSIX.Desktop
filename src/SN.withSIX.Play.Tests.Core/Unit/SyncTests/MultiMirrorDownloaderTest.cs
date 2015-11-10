// <copyright company="SIX Networks GmbH" file="MultiMirrorDownloaderTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class MultiMirrorDownloaderTest
    {
        static readonly Uri httpHost2 = new Uri("http://host2");
        static readonly Uri httpHost1 = new Uri("http://host1");
        static readonly Uri httpTesthost = new Uri("http://testhost");

        static IFileDownloader SetupDefaultDownloader() {
            var downloader = A.Fake<IFileDownloader>();
            A.CallTo(() => downloader.Download(A<Uri>._, A<IAbsoluteFilePath>._))
                .Throws<DownloadException>();
            A.CallTo(() => downloader.DownloadAsync(A<Uri>._, A<IAbsoluteFilePath>._))
                .Throws<DownloadException>();
            return downloader;
        }

        static IMirrorSelector SetupMultiMirrorSelector() {
            var strategy = A.Fake<IMirrorSelector>();
            A.CallTo(() => strategy.GetHost())
                .Throws<HostListExhausted>().Once();
            A.CallTo(() => strategy.GetHost())
                .Returns(httpHost2).Once();
            A.CallTo(() => strategy.GetHost())
                .Returns(httpHost1).Once();

            return strategy;
        }

        static IMirrorSelector SetupDefaultMirrorSelector() {
            var mirrorSelector = A.Fake<IMirrorSelector>();
            A.CallTo(() => mirrorSelector.GetHost())
                .Returns(httpTesthost);
            return mirrorSelector;
        }

        [Test]
        public void CanDownloadFromMultipleMirrors() {
            var mirrorSelector = SetupMultiMirrorSelector();
            var downloader = SetupDefaultDownloader();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            Action act = () => mDownloader.Download(new MultiMirrorFileDownloadSpec("a", "C:\\b".ToAbsoluteFilePath()));

            act.ShouldThrow<HostListExhausted>();
            A.CallTo(() => mirrorSelector.Failure(httpHost1))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mirrorSelector.Failure(httpHost2))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanDownloadFromMultipleMirrorsAsync() {
            var mirrorSelector = SetupMultiMirrorSelector();
            var downloader = SetupDefaultDownloader();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            Func<Task> act =
                () => mDownloader.DownloadAsync(new MultiMirrorFileDownloadSpec("a", "C:\\b".ToAbsoluteFilePath()));

            act.ShouldThrow<HostListExhausted>();
            A.CallTo(() => mirrorSelector.Failure(httpHost1))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mirrorSelector.Failure(httpHost2))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void DownloadFailsWhenVerificationFails() {
            var mirrorSelector = SetupMultiMirrorSelector();
            var downloader = A.Fake<IFileDownloader>();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            Action act =
                () =>
                    mDownloader.Download(new MultiMirrorFileDownloadSpec("a", "C:\\b".ToAbsoluteFilePath(), f => false));

            act.ShouldThrow<HostListExhausted>();
        }

        [Test]
        public void DownloadSucceedsWhenVerificationSucceeds() {
            var mirrorSelector = SetupMultiMirrorSelector();
            var downloader = A.Fake<IFileDownloader>();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            Action act =
                () =>
                    mDownloader.Download(new MultiMirrorFileDownloadSpec("a", "C:\\b".ToAbsoluteFilePath(), f => true));

            act.ShouldNotThrow<HostListExhausted>();
        }


        [Test]
        public async Task SuccessDownloadAsyncShouldBeRegistered() {
            var mirrorSelector = SetupDefaultMirrorSelector();
            var downloader = A.Fake<IFileDownloader>();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            await mDownloader.DownloadAsync(new MultiMirrorFileDownloadSpec("a", @"C:\temp\a".ToAbsoluteFilePath()));

            A.CallTo(() => mirrorSelector.GetHost())
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => downloader.DownloadAsync(new Uri("http://testhost/a"), @"C:\temp\a".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mirrorSelector.Success(httpTesthost))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void SuccessDownloadShouldBeRegistered() {
            var mirrorSelector = SetupDefaultMirrorSelector();
            var downloader = A.Fake<IFileDownloader>();
            var mDownloader = new MultiMirrorFileDownloader(downloader, mirrorSelector);

            mDownloader.Download(new MultiMirrorFileDownloadSpec("a", @"C:\temp\a".ToAbsoluteFilePath()));

            A.CallTo(() => mirrorSelector.GetHost())
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => downloader.Download(new Uri("http://testhost/a"), @"C:\temp\a".ToAbsoluteFilePath()))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mirrorSelector.Success(httpTesthost))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}