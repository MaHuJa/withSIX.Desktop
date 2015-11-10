// <copyright company="SIX Networks GmbH" file="DependencyVersionMatcherTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Packages.Internals;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class DependencyVersionMatcherTest
    {
        [Test]
        public void MatchEqualOrHigher() {
            var dvm = new DependencyVersionMatcher();

            var result = dvm.MatchesConstraints(new[] {"1.0.0.3", "1.0.0.4"}, ">= 1.0.0.4");

            result.Should().Be("1.0.0.4");
        }

        [Test]
        public void MatchEqualOrHigher2() {
            var dvm = new DependencyVersionMatcher();

            var result = dvm.MatchesConstraints(new[] {"1.0.0.4", "1.0.0.5"}, ">= 1.0.0.4");

            result.Should().Be("1.0.0.5");
        }

        [Test]
        public void MatchEqualOrHigher2Reversed() {
            var dvm = new DependencyVersionMatcher();

            var result = dvm.MatchesConstraints(new[] {"1.0.0.5", "1.0.0.4"}, ">= 1.0.0.4");

            result.Should().Be("1.0.0.5");
        }
    }
}