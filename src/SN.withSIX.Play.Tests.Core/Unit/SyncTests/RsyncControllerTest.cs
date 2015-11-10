// <copyright company="SIX Networks GmbH" file="RsyncControllerTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using FakeItEasy;
using NUnit.Framework;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class RsyncControllerTest
    {
        [SetUp]
        public void Setup() {
            _controller = new RsyncController("testsrc", "testremote", @"C:\key", A.Fake<IRsyncLauncher>());
        }

        RsyncController _controller;

        [Test]
        public void CanPull() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run("testremote", "testsrc", @"C:\key"))
                .Returns(new ProcessExitResultWithOutput(0, 0, new ProcessStartInfo(), string.Empty, string.Empty));
            _controller = new RsyncController("testsrc", "testremote", @"C:\key", mock);
            _controller.Pull();
            A.CallTo(() => mock.Run("testremote", "testsrc", @"C:\key"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanPullWithSub() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run(@"testremote\sub", "testsrc", @"C:\key"))
                .Returns(new ProcessExitResultWithOutput(0, 0, new ProcessStartInfo(), string.Empty, string.Empty));
            _controller = new RsyncController("testsrc", "testremote", @"C:\key", mock);
            _controller.Pull("sub");
            A.CallTo(() => mock.Run(@"testremote\sub", "testsrc", @"C:\key")).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanPush() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run("testsrc", "testremote", @"C:\key"))
                .Returns(new ProcessExitResultWithOutput(0, 0, new ProcessStartInfo(), string.Empty, string.Empty));
            _controller = new RsyncController("testsrc", "testremote", @"C:\key", mock);
            _controller.Push();
            A.CallTo(() => mock.Run("testsrc", "testremote", @"C:\key"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanPushWithSub() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run(@"testsrc\sub", "testremote", @"C:\key"))
                .Returns(new ProcessExitResultWithOutput(0, 0, new ProcessStartInfo(), string.Empty, string.Empty));
            _controller = new RsyncController("testsrc", "testremote", @"C:\key", mock);
            _controller.Push("sub");
            A.CallTo(() => mock.Run(@"testsrc\sub", "testremote", @"C:\key"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void PullError() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run("a", "b", null))
                .Returns(new ProcessExitResultWithOutput(1, 0, new ProcessStartInfo(), String.Empty, String.Empty));
            _controller = new RsyncController("b", "a", null, mock);
            Assert.Throws<RsyncException>(() => _controller.Pull());
            A.CallTo(() => mock.Run("a", "b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void PushError() {
            var mock = A.Fake<IRsyncLauncher>();
            A.CallTo(() => mock.Run("a", "b", null))
                .Returns(new ProcessExitResultWithOutput(1, 0, new ProcessStartInfo(), String.Empty, String.Empty));

            _controller = new RsyncController("a", "b", null, mock);
            Assert.Throws<RsyncException>(() => _controller.Push());
            A.CallTo(() => mock.Run("a", "b", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}