// <copyright company="SIX Networks GmbH" file="ServerQueryInfoTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Services;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities
{
    [TestFixture]
    public class ServerQueryInfoTest
    {
        [Test]
        public void CanCreate() {
            var info = new GamespyServersQuery("mytag");
            var info2 = new SourceServersQuery("mytag");
        }

        [Test]
        public void CannotCreateInvalid() {
            var act = new Action(() => new GamespyServersQuery(""));
            var act2 = new Action(() => new GamespyServersQuery(null));

            act.ShouldThrow<ArgumentNullException>();
            act2.ShouldThrow<ArgumentNullException>();
        }
    }
}