// <copyright company="SIX Networks GmbH" file="DependencyVersionMatcher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    public class DependencyVersionMatcher
    {
        static readonly Regex rx = new Regex(@"([\>\<\=\~]*)\s*(\d+\.[\d\.]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string MatchesConstraints(IEnumerable<string> packageVersions, string versionConstraint,
            string branch = null) {
            var doVersion = !string.IsNullOrWhiteSpace(versionConstraint);
            var doBranch = !string.IsNullOrWhiteSpace(branch);
            packageVersions = packageVersions.OrderBy(x => new SpecificVersion("x", x).Version);

            if (doBranch) {
                return doVersion
                    ? packageVersions.LastOrDefault(
                        x =>
                            x.EndsWith("-" + branch) &&
                            MatchesVersionConstraint(x.Split('-').First(), versionConstraint))
                    : packageVersions.LastOrDefault(x => x.EndsWith("-" + branch));
            }

            return doVersion
                ? packageVersions.LastOrDefault(x => MatchesVersionConstraint(x.Split('-').First(), versionConstraint))
                : null;
        }

        static bool MatchesVersionConstraint(string packageVersion, string constraint) {
            var match = rx.Match(constraint);
            if (!match.Success)
                return false;

            var modifier = match.Groups[1].Value;
            var modified = !string.IsNullOrWhiteSpace(modifier);
            var version = match.Groups[2].Value;
            var srcVersion = new Version(packageVersion);
            var destVersion = new Version(version);
            if (!modified)
                return srcVersion == destVersion;

            switch (modifier) {
            case "=":
                return srcVersion == destVersion;
            case ">=":
                return srcVersion >= destVersion;
            case "<=":
                return srcVersion <= destVersion;
            case "<":
                return srcVersion < destVersion;
            case ">":
                return srcVersion > destVersion;
            case "~>":
                return AproxCompare(srcVersion, destVersion);
            default:
                throw new Exception("Unknown modifier: " + modifier);
            }
        }

        static bool AproxCompare(Version srcVersion, Version destVersion) {
            if (destVersion.Major == -1)
                throw new Exception("Invalid version");

            if (srcVersion.Major != destVersion.Major)
                return false;
            if (destVersion.Minor == -1)
                return true;

            if (destVersion.Build == -1)
                return srcVersion.Minor >= destVersion.Minor;

            if (srcVersion.Minor != destVersion.Minor)
                return false;

            if (destVersion.Revision == -1)
                return srcVersion.Build >= destVersion.Build;

            if (srcVersion.Build != destVersion.Build)
                return false;

            return srcVersion.Revision >= destVersion.Revision;
        }
    }
}