// <copyright company="SIX Networks GmbH" file="RepositoryStore.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages.Internals;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    public class RepositoryStore
    {
        readonly DependencyVersionMatcher _dependencyVersionMatcher;

        public RepositoryStore() {
            Objects = new Dictionary<string, string>();
            Packages = new Dictionary<string, List<string>>();
            PackagesContentTypes = new Dictionary<string, List<string>>();
            Bundles = new Dictionary<string, List<string>>();
            PackagesCustomConfigs = new Dictionary<string, PackagesStoreCustomConfigs>();
            _dependencyVersionMatcher = new DependencyVersionMatcher();
        }

        public Dictionary<string, List<string>> Packages { get; set; }
        public Dictionary<string, List<string>> Bundles { get; set; }
        public Dictionary<string, string> Objects { get; set; }
        public Dictionary<string, List<string>> PackagesContentTypes { get; set; }
        public Dictionary<string, PackagesStoreCustomConfigs> PackagesCustomConfigs { get; set; }

        public IDictionary<string, string[]> GetPackages() {
            lock (Packages) {
                return Packages.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }
        }

        public IReadOnlyCollection<string> GetPackageVersions(string package) {
            lock (Packages)
                return Packages[package].ToArray();
        }

        public IEnumerable<string> GetPackagesList() {
            return GetPackagesListAsVersions().Select(x => x.GetFullName());
        }

        public IEnumerable<SpecificVersion> GetPackagesListAsVersions() {
            return GetPackages().SelectMany(x => x.Value.Select(y => new SpecificVersion(x.Key, y)));
        }

        public Dictionary<string, string[]> GetBundles() {
            lock (Bundles) {
                return Bundles.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }
        }

        public IEnumerable<string> GetBundlesList() {
            return GetBundles().SelectMany(x => x.Value.Select(y => x.Key + "-" + y));
        }

        public IEnumerable<SpecificVersion> GetBundlesListAsVersions() {
            return GetBundlesList().Select(x => new SpecificVersion(x));
        }

        public Dictionary<string, string> GetObjects() {
            lock (Objects) {
                return Objects.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public bool HasPackage(string package) {
            return HasPackage(new Dependency(package));
        }

        public bool HasPackage(Dependency package) {
            return GetPackage(package) != null;
        }

        public bool HasPackage(SpecificVersion package) {
            return GetPackage(package) != null;
        }

        public SpecificVersion GetPackage(SpecificVersion package) {
            lock (Packages) {
                if (!Packages.ContainsKey(package.Name))
                    return null;
                var packages = Packages[package.Name];

                if (package.Version == null && package.Branch == null)
                    return GetLatest(package, packages);

                if (packages.Contains(package.VersionData))
                    return package;
                var m = _dependencyVersionMatcher.MatchesConstraints(packages, package.Version.ToString(),
                    package.Branch);
                return m == null ? null : new SpecificVersion(package.Name, m);
            }
        }

        static SpecificVersion GetLatest(BaseVersion package, IEnumerable<string> packages) {
            return new SpecificVersion(package.Name, Dependency.FindLatestPreferNonBranched(packages));
        }

        public SpecificVersion GetPackage(Dependency package) {
            lock (Packages) {
                if (!Packages.ContainsKey(package.Name))
                    return null;
                var packages = Packages[package.Name];

                if (package.Version == null && package.Branch == null)
                    return GetLatest(package, packages);

                if (packages.Contains(package.VersionData))
                    return new SpecificVersion(package.GetFullName());

                var m = _dependencyVersionMatcher.MatchesConstraints(packages, package.Version, package.Branch);
                return m == null ? null : new SpecificVersion(package.Name, m);
            }
        }

        public bool AddPackage(string package) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(package));

            return AddPackage(new SpecificVersion(package));
        }

        public bool AddPackage(SpecificVersion package) {
            Contract.Requires<ArgumentNullException>(package != null);

            lock (Packages) {
                if (!HasPackage(package)) {
                    if (!Packages.ContainsKey(package.Name))
                        Packages.Add(package.Name, new List<string>());
                    Packages[package.Name].Add(package.VersionData);
                    return true;
                }
            }
            return false;
        }

        public bool RemovePackage(string package) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(package));

            return RemovePackage(new SpecificVersion(package));
        }

        public bool RemovePackage(SpecificVersion package) {
            Contract.Requires<ArgumentNullException>(package != null);

            lock (Packages) {
                if (HasPackage(package)) {
                    var packages = Packages[package.Name];
                    packages.Remove(package.VersionData);
                    if (!packages.Any())
                        Packages.Remove(package.Name);
                    return true;
                }
            }
            return false;
        }

        public string[] AddPackage(IEnumerable<string> packages) {
            Contract.Requires<ArgumentNullException>(packages != null);
            return AddPackage(packages.Select(x => new SpecificVersion(x)))
                .Select(x => x.GetFullName()).ToArray();
        }

        public SpecificVersion[] AddPackage(IEnumerable<SpecificVersion> packages) {
            Contract.Requires<ArgumentNullException>(packages != null);
            var added = new List<SpecificVersion>();
            lock (Packages) {
                foreach (var package in packages) {
                    if (!HasPackage(package)) {
                        AddPackage(package);
                        added.Add(package);
                    }
                }
            }
            return added.ToArray();
        }

        public string[] RemovePackage(IEnumerable<string> packages) {
            Contract.Requires<ArgumentNullException>(packages != null);
            var removed = new List<string>();
            lock (Packages) {
                foreach (var package in packages) {
                    if (HasPackage(package)) {
                        RemovePackage(package);
                        removed.Add(package);
                    }
                }
            }
            return removed.ToArray();
        }

        public bool HasBundle(string bundle) {
            return HasBundle(new SpecificVersion(bundle));
        }

        public bool HasBundle(SpecificVersion bundle) {
            lock (Bundles) {
                if (!Bundles.ContainsKey(bundle.Name))
                    return false;
                if (bundle.Version == null && bundle.Branch == null)
                    return true;
                var bundles = Bundles[bundle.Name];
                if (bundles.Contains(bundle.VersionData))
                    return true;
            }
            return false;
        }

        public SpecificVersion GetBundle(SpecificVersion bundle) {
            lock (Bundles) {
                if (!Bundles.ContainsKey(bundle.Name))
                    return null;
                var bundles = Bundles[bundle.Name];

                if (!bundles.Any())
                    return null;

                if (bundle.Version == null && bundle.Branch == null)
                    return new SpecificVersion(bundle.Name, Dependency.FindLatestPreferNonBranched(bundles));

                if (bundles.Contains(bundle.VersionData))
                    return new SpecificVersion(bundle.GetFullName());

                var m = _dependencyVersionMatcher.MatchesConstraints(bundles, bundle.Version.ToString(), bundle.Branch);
                return m == null ? null : new SpecificVersion(bundle.Name, m);
            }
        }

        public SpecificVersion GetBundle(Dependency bundle) {
            lock (Bundles) {
                if (!Bundles.ContainsKey(bundle.Name))
                    return null;
                var bundles = Bundles[bundle.Name];

                if (!bundles.Any())
                    return null;

                if (bundle.Version == null && bundle.Branch == null)
                    return new SpecificVersion(bundle.Name, Dependency.FindLatestPreferNonBranched(bundles));

                if (bundles.Contains(bundle.VersionData))
                    return new SpecificVersion(bundle.GetFullName());

                var m = _dependencyVersionMatcher.MatchesConstraints(bundles, bundle.Version, bundle.Branch);
                return m == null ? null : new SpecificVersion(bundle.Name, m);
            }
        }

        public bool AddBundle(string bundle) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(bundle));

            return AddBundle(new SpecificVersion(bundle));
        }

        public bool AddBundle(SpecificVersion bundle) {
            Contract.Requires<ArgumentNullException>(bundle != null);

            lock (Bundles) {
                if (!HasBundle(bundle)) {
                    if (!Bundles.ContainsKey(bundle.Name))
                        Bundles.Add(bundle.Name, new List<string>());
                    Bundles[bundle.Name].Add(bundle.VersionData);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveBundle(string bundle) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(bundle));

            return RemoveBundle(new SpecificVersion(bundle));
        }

        public bool RemoveBundle(SpecificVersion bundle) {
            Contract.Requires<ArgumentNullException>(bundle != null);

            lock (Bundles) {
                if (HasBundle(bundle)) {
                    var bundles = Bundles[bundle.Name];
                    bundles.Remove(bundle.VersionData);
                    if (!bundles.Any())
                        Bundles.Remove(bundle.Name);
                    return true;
                }
            }
            return false;
        }

        public string[] AddBundle(IEnumerable<string> bundles) {
            Contract.Requires<ArgumentNullException>(bundles != null);
            return AddBundle(bundles.Select(x => new SpecificVersion(x)))
                .Select(x => x.GetFullName()).ToArray();
        }

        public IReadOnlyCollection<SpecificVersion> AddBundle(IEnumerable<SpecificVersion> bundles) {
            Contract.Requires<ArgumentNullException>(bundles != null);
            var added = new List<SpecificVersion>();
            lock (Bundles) {
                foreach (var bundle in bundles) {
                    if (!HasBundle(bundle)) {
                        AddBundle(bundle);
                        added.Add(bundle);
                    }
                }
            }
            return added.ToArray();
        }

        public string[] RemoveBundle(IEnumerable<string> bundles) {
            Contract.Requires<ArgumentNullException>(bundles != null);
            var removed = new List<string>();
            lock (Bundles) {
                foreach (var bundle in bundles) {
                    if (HasBundle(bundle)) {
                        Bundles.Remove(bundle);
                        removed.Add(bundle);
                    }
                }
            }
            return removed.ToArray();
        }

        public ObjectInfo GetObject(string unpackedHash) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(unpackedHash));
            lock (Objects) {
                return Objects.ContainsKey(unpackedHash) ? new ObjectInfo(unpackedHash, Objects[unpackedHash]) : null;
            }
        }

        public ObjectInfo GetObjectByPack(string packedHash) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(packedHash));
            lock (Objects) {
                if (Objects.ContainsValue(packedHash))
                    return new ObjectInfo(Objects.First(x => x.Value.Equals(packedHash)).Key, packedHash);
            }

            return null;
        }

        public ObjectInfo AddObject(string unpackedHash, string packedHash) {
            lock (Objects) {
                var o = GetObject(unpackedHash);
                if (o == null) {
                    o = new ObjectInfo(unpackedHash, packedHash);
                    Objects.Add(o.Checksum, o.ChecksumPack);
                } else
                    o = UpdateObject(o, packedHash);
                return o;
            }
        }

        public ObjectInfo UpdateObject(ObjectInfo o, string packedHash) {
            lock (Objects) {
                Objects[o.Checksum] = packedHash;
            }
            return new ObjectInfo(o.Checksum, packedHash);
        }

        public ObjectInfo UpdateObject(string unpackedHash, string packedHash) {
            var o = GetObject(unpackedHash);
            if (o != null)
                o = UpdateObject(o, packedHash);

            return o;
        }

        public bool RemoveObject(string unpackedHash) {
            lock (Objects) {
                if (Objects.ContainsKey(unpackedHash)) {
                    Objects.Remove(unpackedHash);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveObject(ObjectInfo info) {
            return RemoveObject(info.Checksum);
        }

        public bool RemoveObjectByPack(string packedHash) {
            lock (Objects) {
                var o = GetObjectByPack(packedHash);
                if (o != null) {
                    Objects.Remove(o.Checksum);
                    return true;
                }
            }
            return false;
        }

        public static RepositoryStore FromSeparateStores(RepositoryStoreObjectsDto objects,
            RepositoryStorePackagesDto packages, RepositoryStoreBundlesDto bundles) {
            return new RepositoryStore {
                Objects = objects.Objects.ToDictionary(x => x.Key, x => x.Value),
                Packages = packages.Packages.ToDictionary(x => x.Key, x => x.Value),
                PackagesCustomConfigs =
                    packages.PackagesCustomConfigs.ToDictionary(x => x.Key,
                        x =>
                            new PackagesStoreCustomConfigs {
                                KeepLatestVersions = x.Value.KeepLatestVersions,
                                KeepSpecificBranches = x.Value.KeepSpecificBranches,
                                KeepSpecificVersions = x.Value.KeepSpecificVersions
                            }),
                PackagesContentTypes = packages.PackagesContentTypes.ToDictionary(x => x.Key, x => x.Value),
                Bundles = bundles.Bundles.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}