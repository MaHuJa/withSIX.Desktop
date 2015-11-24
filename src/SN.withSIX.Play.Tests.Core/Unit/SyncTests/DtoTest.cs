// <copyright company="SIX Networks GmbH" file="DtoTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture, Ignore("")]
    public class DtoTest
    {
        [Test]
        public void TestBundle() {
            var dto = new BundleDto {Version = new Version("1.2.1"), Date = new DateTime(2012, 12, 24)};

            Console.WriteLine(dto.ToJson(true));
        }

        [Test]
        public void TestDefaults() {
            var mapped = Repository.MappingEngine.Map<PackageMetaData>(new PackageMetaDataDto());

            mapped.Version.Should().Be(new Version("0.0.1"));
            var current = DateTime.Now;
            mapped.Date.Year.Should().Be(current.Year);
            mapped.Date.Month.Should().Be(current.Month);
            mapped.Date.Day.Should().Be(current.Day);
        }

        [Test]
        public void TestDefaultsDto() {
            var mapped = Repository.MappingEngine.Map<PackageMetaDataDto>(new PackageMetaData("abc"));

            mapped.Version.Should().Be(new Version("0.0.1"));
            var current = DateTime.Now;
            mapped.Date.Value.Year.Should().Be(current.Year);
            mapped.Date.Value.Month.Should().Be(current.Month);
            mapped.Date.Value.Day.Should().Be(current.Day);
        }

        [Test]
        public void TestPackage() {
            var dto = new PackageMetaDataDto {Version = new Version("1.2.1"), Date = new DateTime(2012, 12, 24)};

            Console.WriteLine(dto.ToJson(true));
        }

        [Test]
        public void TestSameInstance() {
            var package = new PackageMetaData("abc") {
                Additional = new Dictionary<string, string>(),
                Authors = new Dictionary<string, string> {{"abc", "cba"}}
            };
            var mappedPackage = Repository.MappingEngine.Map<PackageMetaData>(package);

            mappedPackage.Additional.Should().Equal(package.Additional);
            mappedPackage.Authors.Should().Equal(package.Authors);

            ReferenceEquals(mappedPackage.Additional, package.Additional).Should().BeFalse();
            ReferenceEquals(mappedPackage.Authors, package.Authors).Should().BeFalse();
        }

        [Test]
        public void TestSameInstanceDto() {
            var package = new PackageMetaData("abc") {
                Additional = new Dictionary<string, string>(),
                Authors = new Dictionary<string, string> {{"abc", "cba"}}
            };
            var mappedPackage = Repository.MappingEngine.Map<PackageMetaDataDto>(package);

            mappedPackage.Additional.Should().Equal(package.Additional);
            mappedPackage.Authors.Should().Equal(package.Authors);

            ReferenceEquals(mappedPackage.Additional, package.Additional).Should().BeFalse();
            ReferenceEquals(mappedPackage.Authors, package.Authors).Should().BeFalse();
        }
    }
}