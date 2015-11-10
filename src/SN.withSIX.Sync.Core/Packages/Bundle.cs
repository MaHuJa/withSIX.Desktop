// <copyright company="SIX Networks GmbH" file="Bundle.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Core.Packages
{
    public enum BundleScope
    {
        All,
        Server,
        Client
    }

    public enum BundleGroup
    {
        All,
        Server,
        Client
    }

    public class Bundle : MetaDataBase, IComparePK<Bundle>
    {
        public static readonly BundleFactory Factory = new BundleFactory();

        public Bundle(SpecificVersion version) : base(version) {
            Required = new Dictionary<string, string>();
            RequiredClients = new Dictionary<string, string>();
            RequiredServers = new Dictionary<string, string>();
            Optional = new Dictionary<string, string>();
            OptionalClients = new Dictionary<string, string>();
            OptionalServers = new Dictionary<string, string>();
        }

        public Bundle(string fullyQualifiedName) : this(new SpecificVersion(fullyQualifiedName)) {}
        public Dictionary<string, string> Required { get; set; }
        public Dictionary<string, string> RequiredClients { get; set; }
        public Dictionary<string, string> RequiredServers { get; set; }
        public Dictionary<string, string> Optional { get; set; }
        public Dictionary<string, string> OptionalClients { get; set; }
        public Dictionary<string, string> OptionalServers { get; set; }

        public override bool ComparePK(object other) {
            var o = other as Bundle;
            if (o == null) {
                var o2 = other as Dependency;
                if (o2 == null)
                    return false;
                return ComparePK((Dependency) other);
            }
            return ComparePK((Bundle) other);
        }

        public bool ComparePK(Bundle other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public Dictionary<string, string> GetRequiredClient() {
            return Required.Concat(RequiredClients).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<string, string> GetRequiredServer() {
            return Required.Concat(RequiredServers).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dependency[] GetRequiredClientAsDeps() {
            return GetRequiredClient().Select(x => new Dependency(x.Key, x.Value)).ToArray();
        }

        public Dependency[] GetRequiredServerAsDeps() {
            return GetRequiredServer().Select(x => new Dependency(x.Key, x.Value)).ToArray();
        }

        public Dictionary<string, string> GetOptionalClient() {
            return Optional.Concat(OptionalClients).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<string, string> GetOptionalServer() {
            return Optional.Concat(OptionalServers).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dependency[] GetOptionalClientAsDeps() {
            return GetOptionalClient().Select(x => new Dependency(x.Key, x.Value)).ToArray();
        }

        public Dependency[] GetOptionalServerAsDeps() {
            return GetOptionalServer().Select(x => new Dependency(x.Key, x.Value)).ToArray();
        }

        public Dictionary<string, string> GetAllPackages(BundleScope scope, bool includeOptional = false) {
            IEnumerable<KeyValuePair<string, string>> packages = new Collection<KeyValuePair<string, string>>();

            if (includeOptional)
                packages = GetOptionalPackages(scope);

            return packages.Concat(GetPackages(scope)).ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<string, string> GetPackages(BundleScope scope) {
            switch (scope) {
            case BundleScope.All:
                return
                    GetPackages(BundleGroup.All)
                        .Concat(GetPackages(BundleGroup.Client))
                        .Concat(GetPackages(BundleGroup.Server))
                        .ToDictionary(x => x.Key, x => x.Value);
            case BundleScope.Server:
                return
                    GetPackages(BundleGroup.All)
                        .Concat(GetPackages(BundleGroup.Server))
                        .ToDictionary(x => x.Key, x => x.Value);
            case BundleScope.Client:
                return
                    GetPackages(BundleGroup.All)
                        .Concat(GetPackages(BundleGroup.Client))
                        .ToDictionary(x => x.Key, x => x.Value);
            }

            return null;
        }

        Dictionary<string, string> GetPackages(BundleGroup group) {
            switch (group) {
            case BundleGroup.All:
                return Required;
            case BundleGroup.Client:
                return RequiredClients;
            case BundleGroup.Server:
                return RequiredServers;
            }
            return null;
        }

        public Dictionary<string, string> GetAllPackages(BundleGroup group) {
            var packages = new Dictionary<string, string>();
            packages = Dependencies.Select(c => new Bundle("all-packages"))
                .Aggregate(packages,
                    (current, col) =>
                        current.Concat(col.GetAllPackages(group))
                            .ToDictionary(x => x.Key, x => x.Value));
            return packages.Concat(GetPackages(group)).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<string, string> GetAllOptionalPackages(BundleScope scope) {
            var packages = new Dictionary<string, string>();
            packages = Dependencies.Select(c => new Bundle("optional-packages"))
                .Aggregate(packages,
                    (current, col) =>
                        current.Concat(col.GetAllOptionalPackages(scope))
                            .ToDictionary(x => x.Key, x => x.Value));
            return packages.Concat(GetOptionalPackages(scope)).ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<string, string> GetOptionalPackages(BundleScope scope) {
            switch (scope) {
            case BundleScope.All:
                return
                    GetOptionalPackages(BundleGroup.All)
                        .Concat(GetOptionalPackages(BundleGroup.Client))
                        .Concat(GetOptionalPackages(BundleGroup.Server))
                        .ToDictionary(x => x.Key, x => x.Value);
            case BundleScope.Server:
                return
                    GetOptionalPackages(BundleGroup.All)
                        .Concat(GetOptionalPackages(BundleGroup.Server))
                        .ToDictionary(x => x.Key, x => x.Value);
            case BundleScope.Client:
                return
                    GetOptionalPackages(BundleGroup.All)
                        .Concat(GetOptionalPackages(BundleGroup.Client))
                        .ToDictionary(x => x.Key, x => x.Value);
            }

            return null;
        }

        Dictionary<string, string> GetOptionalPackages(BundleGroup group) {
            switch (group) {
            case BundleGroup.All:
                return Optional;
            case BundleGroup.Client:
                return OptionalClients;
            case BundleGroup.Server:
                return OptionalServers;
            }
            return null;
        }

        public static Bundle Load(IAbsoluteFilePath filePath) {
            return Repository.Load<BundleDto, Bundle>(filePath);
        }
    }

    public class BundleFactory
    {
        public Bundle Open(IAbsoluteFilePath filePath) {
            return Bundle.Load(filePath);
        }
    }
}