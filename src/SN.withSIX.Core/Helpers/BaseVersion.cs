// <copyright company="SIX Networks GmbH" file="BaseVersion.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using SN.withSIX.Core.Properties;

namespace SN.withSIX.Core.Helpers
{
    // TODO: Stronger validation of each element: Name, Version, Branch (Dependency has version constraint support though..)
    public abstract class BaseVersion : IEquatable<BaseVersion>
    {
        public static readonly Regex RxPackageName =
            new Regex(@"(.*)((\-\d+\.[\d\.]+)(\-\w+)|(\-\d+\.[\d\.]+))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string Name { get; protected set; }
        public string Branch { get; protected set; }
        public string VersionData { get; protected set; }

        public virtual string DisplayName => VersionData;

        public bool Equals(BaseVersion other) {
            return other != null && GetFullName().Equals(other.GetFullName());
        }

        public abstract string GetFullName();

        public override bool Equals(object other) {
            return Equals(other as BaseVersion);
        }

        public override int GetHashCode() {
            return GetFullName().GetHashCode();
        }

        public override string ToString() {
            return GetFullName();
        }

        protected static string JoinConstraints(IEnumerable<string> constraints) {
            return String.Join("-", constraints.Where(x => !String.IsNullOrWhiteSpace(x)));
        }
    }

    public class Dependency : BaseVersion, IComparePK<Dependency>
    {
        static readonly Regex rxDependency =
            new Regex(@"(.*)((\-([\>\<\=\~]*)\s*\d+\.[\d\.]+)(\-\w+)|(\-([\>\<\=\~]*)\s*\d+\.[\d\.]+))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        string _fullName;

        public Dependency(string fullyQualifiedName) {
            Contract.Requires<ArgumentNullException>(fullyQualifiedName != null);
            ParseFullyQualifiedName(fullyQualifiedName);
            VersionData = GetVersionData();
        }

        public Dependency(string name, string version) {
            Contract.Requires<ArgumentNullException>(name != null);
            Name = name;
            ParseVersion(version);

            VersionData = GetVersionData();
        }

        public Dependency(string name, string version, string branch) {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<NotSupportedException>(branch == null || version != null,
                "Cannot specify a branch if no version is specified");
            Name = name;
            Version = String.IsNullOrWhiteSpace(version) ? null : version;
            Branch = String.IsNullOrWhiteSpace(branch) || branch.ToLower() == "stable" ? null : branch.ToLower();

            VersionData = GetVersionData();
        }

        public string Version { get; protected set; }

        public bool ComparePK(object other) {
            var o = other as Dependency;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(Dependency other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        void ParseFullyQualifiedName(string fullName) {
            var match = rxDependency.Match(fullName);
            if (match.Success) {
                Name = match.Groups[1].Value;
                if (!String.IsNullOrWhiteSpace(match.Groups[5].Value)) {
                    var branch = match.Groups[5].Value.Substring(1).ToLower();
                    if (branch != "stable")
                        Branch = branch;
                }
                if (String.IsNullOrWhiteSpace(match.Groups[6].Value)) {
                    Version = !String.IsNullOrWhiteSpace(match.Groups[3].Value)
                        ? match.Groups[3].Value.Substring(1)
                        : null;
                } else
                    Version = match.Groups[6].Value.Substring(1);
            } else
                Name = fullName;
        }

        void ParseVersion(string version) {
            var v = version.Split('-');
            version = v[0];
            if (v.Length > 1)
                Branch = v[1].ToLower();
            Version = version;
        }

        // TODO: protect VersionData. Why should it ever be an empty string? It is either null, or it is a valid value...
        string GetVersionData() {
            var versionData = Version;
            if (String.IsNullOrWhiteSpace(Branch))
                return versionData;

            if (Version == null)
                throw new NotSupportedException("Cannot specify a branch if no version is specified");
            versionData += "-" + Branch;
            return versionData;
        }

        static IEnumerable<SpecificVersion> GetOrderedVersions(IEnumerable<SpecificVersion> dependencies) {
            return
                dependencies.OrderByDescending(
                    x => x.Branch == null || x.Branch.ToLower() == "stable" ? "stable" : x.Branch)
                    .ThenByDescending(x => x.Version);
        }

        public static SpecificVersion FindLatestPreferNonBranched(IEnumerable<SpecificVersion> dependencies) {
            return GetOrderedVersions(dependencies).FirstOrDefault();
        }

        public static string FindLatestPreferNonBranched(IEnumerable<string> items) {
            var sortedItems = items.OrderByDescending(x => new SpecificVersion("x", x).Version).ToArray();
            return sortedItems.FirstOrDefault(x => !x.Contains("-") || x.Contains("-stable")) ??
                   sortedItems.FirstOrDefault();
        }

        public string GetConstraints(IEnumerable<string> inputConstraints = null) {
            var constraints = inputConstraints == null ? new List<string>() : inputConstraints.ToList();
            if (Version != null)
                constraints.Add(Version);
            if (String.IsNullOrWhiteSpace(Branch))
                return JoinConstraints(constraints);

            var b = Branch.ToLower();
            if (b != "stable")
                constraints.Add(b);
            return JoinConstraints(constraints);
        }

        public override string GetFullName() {
            return _fullName ?? (_fullName = GetConstraints(new List<string> {Name}));
        }

        public static SpecificVersion FindLatest(IEnumerable<SpecificVersion> packages) {
            return
                packages.OrderByDescending(x => x.Version).ThenByDescending(x => x.Branch).FirstOrDefault();
        }
    }

    public class SpecificVersion : BaseVersion, IComparable<SpecificVersion>, IEquatable<SpecificVersion>,
        IComparePK<SpecificVersion>
    {
        public const string DefaultVersion = "0.0.1";
        public static readonly Version DefaultV = new Version(DefaultVersion);
        string _fullName;

        public SpecificVersion(string fullyQualifiedName) {
            ParseFullyQualifiedName(fullyQualifiedName);
            VersionData = GetVersionData();
        }

        public SpecificVersion(string name, string versionData) {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(versionData != null);
            Name = name;
            ParseVersion(versionData);
            VersionData = GetVersionData();
        }

        public SpecificVersion(string name, Version version, string branch)
            : this(name, version) {
            Contract.Requires<ArgumentNullException>(branch != null);
            Branch = branch;
            VersionData = GetVersionData();
        }

        public SpecificVersion(string name, Version version) {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(version != null);

            Name = name;
            Version = version;
            VersionData = GetVersionData();
        }

        [NotNull]
        public Version Version { get; private set; }

        public int CompareTo(SpecificVersion other) {
            if (other == null)
                return 1;
            var versionCompare = Version.CompareTo(other.Version);

            if (versionCompare == 1 || versionCompare == -1)
                return versionCompare;

            var myBranch = string.IsNullOrWhiteSpace(Branch) ? "stable" : Branch;
            var otherBranch = string.IsNullOrWhiteSpace(other.Branch) ? "stable" : other.Branch;
            if (myBranch == "stable" && otherBranch == myBranch)
                return 0;
            if (myBranch == "stable")
                return 1;
            if (otherBranch == "stable")
                return -1;
            return myBranch.CompareTo(otherBranch);
        }

        public bool ComparePK(object other) {
            var o = other as SpecificVersion;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(SpecificVersion other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public bool Equals(SpecificVersion other) {
            return other != null && (ReferenceEquals(this, other) || ComparePK(other));
        }

        public override int GetHashCode() {
            return GetFullName().GetHashCode();
        }

        public Dependency ToDependency() {
            return new Dependency(GetFullName());
        }

        string GetVersionData() {
            var versionData = Version.ToString();
            if (!String.IsNullOrWhiteSpace(Branch))
                versionData += "-" + Branch;
            return versionData;
        }

        void ParseFullyQualifiedName(string fullName) {
            var match = RxPackageName.Match(fullName);
            if (match.Success) {
                Name = match.Groups[1].Value;
                if (!String.IsNullOrWhiteSpace(match.Groups[4].Value)) {
                    var branch = match.Groups[4].Value.Substring(1).ToLower();
                    if (branch != "stable")
                        Branch = branch;
                }
                if (String.IsNullOrWhiteSpace(match.Groups[5].Value)) {
                    Version = !String.IsNullOrWhiteSpace(match.Groups[3].Value)
                        ? Version.Parse(match.Groups[3].Value.Substring(1))
                        : null;
                } else
                    Version = Version.Parse(match.Groups[5].Value.Substring(1));
            } else
                Name = fullName;

            if (Version == null)
                Version = DefaultV;
        }

        void ParseVersion(string versionData) {
            var versions = versionData.Split('-');
            Version = Version.Parse(versions.First());
            Branch = versions.Length > 1 ? versions[1] : null;
        }

        public override string GetFullName() {
            return _fullName ?? (_fullName = GetConstraints(new List<string> {Name}));
        }

        public string GetConstraints(IEnumerable<string> inputConstraints = null) {
            var constraints = inputConstraints == null ? new List<string>() : inputConstraints.ToList();
            if (Version != null)
                constraints.Add(Version.ToString());
            if (String.IsNullOrWhiteSpace(Branch))
                return JoinConstraints(constraints);

            var b = Branch.ToLower();
            if (b != "stable")
                constraints.Add(b);
            return JoinConstraints(constraints);
        }

        public static bool operator ==(SpecificVersion a, SpecificVersion b) {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
                return false;

            // Return true if the fields match:
            return a.ComparePK(b);
        }

        public static bool operator !=(SpecificVersion a, SpecificVersion b) {
            return !(a == b);
        }
    }

    public static class VersionExtensions
    {
        public static SpecificVersion ToSpecificVersion(this BaseVersion version) {
            return new SpecificVersion(version.GetFullName());
        }
    }
}