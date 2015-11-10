// <copyright company="SIX Networks GmbH" file="ScoreMirrorSelectorTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests.MirrorSelectors
{
    [TestFixture]
    public class ScoreMirrorSelectorTest
    {
        [SetUp]
        public void Setup() {
            _strategy = new ScoreMirrorSelector(GetHostChecker(), defaultHostPool);
        }

        static HostChecker GetHostChecker() {
            return new HostChecker(() => ProtocolPreference.Any);
        }

        ScoreMirrorSelector _strategy;
        static readonly Uri httpHost = new Uri("http://a");
        static readonly Uri rsyncHost = new Uri("rsync://b");
        static readonly Uri zsyncHost = new Uri("zsync://c");
        static readonly Uri[] defaultHostPool = {httpHost, rsyncHost, zsyncHost};
        static readonly Uri nonExistent = new Uri("http://nonexistent");

        class TimeTravelTestScoreMirrorSelector : ScoreMirrorSelector
        {
            public TimeTravelTestScoreMirrorSelector(IReadOnlyCollection<Uri> hostPool)
                : base(GetHostChecker(), hostPool) {
                FailedScoreIncreaseEvery = TimeSpan.FromSeconds(1);
            }
        }

        void FailMultiplier(Uri host, int times) {
            for (var i = 0; i < times; i++)
                _strategy.Failure(host);
        }

        [Test]
        public void CanGetMirrorFromThePool() {
            var host = _strategy.GetHost();

            host.Should().Be(httpHost, "because it is the first in the list");
        }

        [Test]
        public void CanRegisterFailure() {
            Action act = () => _strategy.Failure(httpHost);

            act.ShouldNotThrow("because it should be a valid host");
        }

        [Test]
        public void CanRegisterSuccess() {
            Action act = () => _strategy.Success(httpHost);

            act.ShouldNotThrow("because it should be a valid host");
        }

        [Test, Category("Slow")]
        public void FailedHostIncreasesOnePointPerMinute() {
            _strategy = new TimeTravelTestScoreMirrorSelector(defaultHostPool);
            FailMultiplier(httpHost, 2);

            var host1 = _strategy.GetHost();
            Thread.Sleep(4000);
            var host2 = _strategy.GetHost();

            host1.Should().Be(rsyncHost, "because it should have higher score");
            host2.Should().Be(httpHost, "because it should have same score again");
        }

        [Test]
        public void HigherScoreMirrorWins() {
            _strategy.Success(rsyncHost);

            var host = _strategy.GetHost();

            host.Should().Be(rsyncHost, "because it has highest score");
        }

        [Test]
        public void LowerScoreMirrorLooses() {
            _strategy.Failure(httpHost);

            var host = _strategy.GetHost();

            host.Should().NotBe(httpHost, "because it has lower score");
        }

        [Test]
        public void ThrowsOnCreateWithEmptyHostList() {
            Action act = () => _strategy = new ScoreMirrorSelector(GetHostChecker(), new Uri[0]);

            act.ShouldThrow<EmptyHostList>();
        }

        [Test]
        public void ThrowsOnFailingNonExistentMirror() {
            Action act = () => _strategy.Failure(nonExistent);

            act.ShouldThrow<NoSuchMirror>();
        }

        [Test]
        public void ThrowsOnNoMoreHosts() {
            FailMultiplier(httpHost, 1000);
            FailMultiplier(rsyncHost, 1000);
            FailMultiplier(zsyncHost, 1000);

            Action act = () => _strategy.GetHost();

            act.ShouldThrow<HostListExhausted>();
        }

        [Test]
        public void ThrowsOnNullHostList() {
            Action act = () => _strategy = new ScoreMirrorSelector(GetHostChecker(), null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ThrowsOnSucceedingNonExistentMirror() {
            Action act = () => _strategy.Success(nonExistent);

            act.ShouldThrow<NoSuchMirror>();
        }
    }
}