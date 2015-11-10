// <copyright company="SIX Networks GmbH" file="PackageHelperTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Sync.Core.Packages;

namespace SN.withSIX.Play.Tests.Core.Unit
{
    [TestFixture]
    public class PackageHelperTest
    {
        [Test]
        public void TestPackify() {
            var input = "[CO06] Behind Enemy Lines v1.0";

            var output = PackageHelper.Packify(input);

            output.Should().Be("co06-behind_enemy_lines_v1-0");
        }
    }
}