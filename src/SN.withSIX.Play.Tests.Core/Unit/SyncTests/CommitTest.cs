// <copyright company="SIX Networks GmbH" file="CommitTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class CommitTest
    {
        [Test]
        public void TestCommitOrder() {
            var list = new[] {"@cba-1.0.1", "@cba-1.1.1", "@cba-1.1", "@cba-1.0", "@cba-1"};

            var orderedList = list.OrderBy(x => x);

            orderedList.Should().Equal(new[] {"@cba-1", "@cba-1.0", "@cba-1.0.1", "@cba-1.1", "@cba-1.1.1"});
        }
    }
}