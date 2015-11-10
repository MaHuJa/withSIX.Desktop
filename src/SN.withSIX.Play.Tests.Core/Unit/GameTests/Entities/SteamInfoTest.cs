// <copyright company="SIX Networks GmbH" file="SteamInfoTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities
{
    [TestFixture]
    public class SteamInfoTest
    {
        [Test]
        public void CanCreate() {
            var info = new SteamInfo(1, "some folder");
            info.AppId.Should().Be(1);
            info.Folder.Should().Be("some folder");
        }

        [Test]
        public void CannotCreateWithInvalidAppId() {
            var act = new Action(() => new SteamInfo(-1, "some folder"));
            var act2 = new Action(() => new SteamInfo(-500, "some folder"));

            act.ShouldThrow<ArgumentOutOfRangeException>();
            act2.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void CannotCreateWithInvalidFolder() {
            var act = new Action(() => new SteamInfo(0, null));
            var act2 = new Action(() => new SteamInfo(0, ""));

            act.ShouldThrow<ArgumentNullException>();
            act2.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void NullObjectPattern() {
            var info = new NullSteamInfo();

            info.AppId.Should().Be(-1);
            info.Folder.Should().Be(null);
        }
    }
}