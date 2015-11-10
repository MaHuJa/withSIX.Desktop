// <copyright company="SIX Networks GmbH" file="RegistryInfoTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities
{
    [TestFixture]
    public class RegistryInfoTest
    {
        [Test]
        public void CanCreate() {
            var info = new RegistryInfo("some path", "some key");
            info.Path.Should().Be("some path");
            info.Key.Should().Be("some key");
        }

        [Test]
        public void CannotCreateWithInvalidAppId() {
            var act = new Action(() => new RegistryInfo(null, "some key"));
            var act2 = new Action(() => new RegistryInfo("", "some key"));

            act.ShouldThrow<ArgumentNullException>();
            act2.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void CannotCreateWithInvalidFolder() {
            var act = new Action(() => new RegistryInfo("some path", null));
            var act2 = new Action(() => new RegistryInfo("some path", ""));

            act.ShouldThrow<ArgumentNullException>();
            act2.ShouldNotThrow();
        }

        [Test]
        public void NullObjectPattern() {
            var info = new NullRegistryInfo();

            info.Path.Should().Be(null);
            info.Key.Should().Be(null);
        }
    }
}