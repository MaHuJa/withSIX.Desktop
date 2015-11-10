// <copyright company="SIX Networks GmbH" file="DependencyTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Tests.Core.Unit.SyncTests
{
    [TestFixture]
    public class DependencyTest
    {
        [Test]
        public void FindLatestNOnBranches() {
            var deps = new[] {"0.0.9", "0.0.1", "0.0.10"};
            var dep = Dependency.FindLatestPreferNonBranched(deps);

            dep.Should().Be("0.0.10");
        }

        [Test]
        public void JustName() {
            var dep = new Dependency("@cba_a3");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().BeNull();
            dep.Branch.Should().BeNull();
        }

        [Test]
        public void SpecialName() {
            var dep = new Dependency("@cba_a3-xx-1-1-1.0.2");

            dep.Name.Should().Be("@cba_a3-xx-1-1");
            dep.Version.Should().Be("1.0.2");
            dep.Branch.Should().BeNull();
        }

        [Test]
        public void SpecialName2() {
            var dep = new Dependency("@cba_a3-xx-1-1-1.0.2-beta");

            dep.Name.Should().Be("@cba_a3-xx-1-1");
            dep.Version.Should().Be("1.0.2");
            dep.Branch.Should().Be("beta");
        }

        [Test]
        public void SpecialVersion() {
            var dep = new Dependency("@cba_a3-1983.12.24");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be("1983.12.24");
            dep.Branch.Should().BeNull();
        }

        [Test]
        public void SpecialVersionConstraints() {
            var dep = new Dependency("@cba_a3->=1.0.0.4");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be(">=1.0.0.4");
            dep.Branch.Should().BeNull();
        }

        [Test]
        public void SpecialVersionConstraintsWithBranch() {
            var dep = new Dependency("@cba_a3->=1.0.0.4-beta");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be(">=1.0.0.4");
            dep.Branch.Should().Be("beta");
        }

        [Test]
        public void Standard() {
            var dep = new Dependency("@cba_a3-1.0.0.4");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be("1.0.0.4");
            dep.Branch.Should().BeNull();
        }

        [Test]
        public void StandardWithBranch() {
            var dep = new Dependency("@cba_a3-1.0.0.4-beta");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be("1.0.0.4");
            dep.Branch.Should().Be("beta");
        }

        [Test]
        public void StandardWithStableBranch() {
            var dep = new Dependency("@cba_a3-1.0.0.4-stable");

            dep.Name.Should().Be("@cba_a3");
            dep.Version.Should().Be("1.0.0.4");
            dep.Branch.Should().BeNull();
        }
    }


    public class SpecificVersion
    {
        public string Branch;
        public Version Version;
        public SpecificVersion() {}

        public SpecificVersion(string fullyQualifiedName) {
            var split = fullyQualifiedName.Split('-');
            if (split.Length == 0 || split.Length > 2)
                throw new Exception("Invalid length: " + split.Length);

            Version = split[0].ToVersion();
            if (split.Length > 1)
                Branch = split[1];
        }

        public SpecificVersion(Version version, string branch) {
            Version = version;
            Branch = branch;
        }
    }

    public class SpecificReference : SpecificVersion
    {
        public string Name;
        public SpecificReference() {}

        public SpecificReference(string name, Version version, string branch)
            : base(version, branch) {
            Name = name;
        }

        public SpecificReference(string fullyQualifiedName) {
            var split = fullyQualifiedName.Split('-');
            if (split.Length == 0 || split.Length > 3)
                throw new Exception("Invalid length: " + split.Length);

            Name = split[0];

            if (split.Length > 1)
                Version = split[1].ToVersion();
            if (split.Length > 2)
                Branch = split[2];
        }
    }

    public class DependencyReference : SpecificReference
    {
        public string Constraint;
        public DependencyReference() {}

        public DependencyReference(string name, string constraint, Version version, string branch)
            : base(name, version, branch) {
            Constraint = constraint;
        }

        public DependencyReference(string fullyQualifiedName) {
            var split = fullyQualifiedName.Split('-');
            if (split.Length == 0 || split.Length > 3)
                throw new Exception("Invalid length: " + split.Length);

            Name = split[0];

            if (split.Length > 1)
                Version = split[1].ToVersion();
            if (split.Length > 2)
                Branch = split[2];
        }
    }
}