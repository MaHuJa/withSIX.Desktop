// <copyright company="SIX Networks GmbH" file="PackageItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public static class VersionKeyWords
    {
        public const string Latest = "latest-prefer-stable";
        public const string LatestInclPreRelease = "latest-any";
        public const string Global = "global";
    }

    public class LatestDependency : SpecializedDependency
    {
        public LatestDependency(string name)
            : base(name, VersionKeyWords.Latest) {
            //VersionData = VersionKeyWords.Latest;
        }

        public override string DisplayName
        {
            get { return "Latest (prefer stable)"; }
        }
    }

    public abstract class SpecializedDependency : Dependency
    {
        protected SpecializedDependency(string name, string version) : base(name, version) {}
    }

    public class GlobalDependency : SpecializedDependency
    {
        public GlobalDependency(string name)
            : base(name, VersionKeyWords.Global) {
            //VersionData = VersionKeyWords.Global;
        }

        public override string DisplayName
        {
            get { return "Global (default)"; }
        }
    }

    public class AnyLatestDependency : SpecializedDependency
    {
        public AnyLatestDependency(string name)
            : base(name, VersionKeyWords.LatestInclPreRelease) {
            //VersionData = VersionKeyWords.LatestInclPreRelease;
        }

        public override string DisplayName
        {
            get { return "Latest (any)"; }
        }
    }

    public class PackageItem : PropertyChangedBase, IComparePK<PackageItem>
    {
        readonly GlobalDependency _globalJoker;
        readonly AnyLatestDependency _latestAnyJoker;
        readonly Dependency _latestJoker;
        SpecificVersion _actualDependency;
        PackageMetaData _current;
        Dependency _currentDependency;
        BaseVersion _currentLocalDependency;
        SpecificVersion _currentVersion;
        bool _isLatestSelected;
        bool _isPinned;
        bool _versioned;

        public PackageItem(string name, RepositoryHandler handler, IEnumerable<SpecificVersion> packages) {
            Name = name;
            _globalJoker = new GlobalDependency(name);
            _latestJoker = new LatestDependency(name);
            _latestAnyJoker = new AnyLatestDependency(name);
            ActualPackages = new ReactiveList<SpecificVersion>(packages);
            Packages =
                new ReactiveList<Dependency>(
                    new[] {_latestJoker, _latestAnyJoker}.Concat(
                        ActualPackages.Select(x => new Dependency(x.GetFullName()))));
            LocalPackages =
                new ReactiveList<Dependency>(
                    new[] {_globalJoker}.Concat(Packages));
            Handler = handler;
            SetInitialDependency(name);
        }

        public PackageItem(PackageMetaData metaData, RepositoryHandler handler, IEnumerable<SpecificVersion> packages)
            : this(metaData.Name, handler, packages) {
            ActualDependency = metaData.ToSpecificVersion();
            Current = metaData;
        }

        public ReactiveList<Dependency> LocalPackages { get; }
        public SpecificVersion CurrentVersion
        {
            get { return _currentVersion; }
            set { SetProperty(ref _currentVersion, value); }
        }
        public RepositoryHandler Handler { get; protected set; }
        public string Name { get; protected set; }
        public ReactiveList<Dependency> Packages { get; }
        public ReactiveList<SpecificVersion> ActualPackages { get; }
        public PackageMetaData Current
        {
            get { return _current; }
            private set { SetProperty(ref _current, value); }
        }
        public Dependency CurrentDependency
        {
            get { return _currentDependency; }
            set
            {
                if (!SetProperty(ref _currentDependency, value))
                    return;
                UpdateActual();
                SaveDesiredVersion();
            }
        }
        public BaseVersion CurrentLocalDependency
        {
            get { return _currentLocalDependency; }
            set
            {
                if (!SetProperty(ref _currentLocalDependency, value))
                    return;
                UpdateActual();
            }
        }
        public SpecificVersion ActualDependency
        {
            get { return _actualDependency; }
            set { SetProperty(ref _actualDependency, value); }
        }
        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }
        public bool IsLatestSelected
        {
            get { return _isLatestSelected; }
            set { SetProperty(ref _isLatestSelected, value); }
        }

        public bool ComparePK(object other) {
            var o = other as PackageItem;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(PackageItem other) {
            return other != null && other.Name != null && other.Name.Equals(Name);
        }

        void SetInitialDependency(string name) {
            var desired = DomainEvilGlobal.Settings.GameOptions.GetDesiredPackageVersion(name);
            switch (desired) {
            case null:
                CurrentDependency = _latestJoker;
                break;
            case VersionKeyWords.LatestInclPreRelease:
                CurrentDependency = _latestAnyJoker;
                break;
            default:
                CurrentDependency = FindDesiredOrJoker(desired);
                break;
            }
        }

        void UpdateActual() {
            var currentLocalDependency = CurrentLocalDependency;
            var current = currentLocalDependency == null || currentLocalDependency == _globalJoker
                ? CurrentDependency
                : currentLocalDependency;
            ActualDependency = GetActual(current);
            IsLatestSelected = current == _latestJoker || current == _latestAnyJoker;
        }

        Dependency FindDesiredOrJoker(string desired) {
            return Packages.FirstOrDefault(x => x.VersionData.Equals(desired))
                   ?? _latestJoker;
        }

        void SaveDesiredVersion() {
            var value = CurrentDependency;
            DomainEvilGlobal.Settings.GameOptions.SetDesiredPackageVersion(Name,
                value == null || value == _latestJoker ? null : value.VersionData);
        }

        SpecificVersion GetCurrentVersion() {
            var current = ActualDependency;
            IAbsoluteDirectoryPath folder;
            if (_versioned) {
                if (current == null)
                    return null;
                folder = Handler.BundleManager.WorkDir.GetChildDirectoryWithName(current.GetFullName());
            } else
                folder = Handler.BundleManager.WorkDir.GetChildDirectoryWithName(Name);

            return Package.ReadSynqInfoFile(folder);
        }

        public SpecificVersion UpdateCurrentVersion() {
            return CurrentVersion = GetCurrentVersion();
        }

        public SpecificVersion GetLatestDependency() {
            return Dependency.FindLatestPreferNonBranched(ActualPackages);
        }

        public SpecificVersion GetLatestAnyDependency() {
            return Dependency.FindLatest(ActualPackages);
        }

        SpecificVersion GetActual(BaseVersion current) {
            if (current == _latestJoker)
                return GetLatestDependency();

            return current == _latestAnyJoker ? GetLatestAnyDependency() : current.ToSpecificVersion();
        }

        public void UpdateCurrentIfAvailable() {
            var package = ActualDependency;
            var hasPackage = Handler.Repository.HasPackage(package);
            if (!hasPackage)
                package = null;
            Current = package != null ? Handler.PackageManager.GetMetaData(package) : null;
        }

        public async Task<PackageMetaData> UpdateCurrent() {
            var package = ActualDependency;
            var hasPackage = Handler.Repository.HasPackage(package);
            if (!hasPackage) {
                if (Handler.Remote)
                    await Handler.PackageManager.GetAndAddPackage(package).ConfigureAwait(false);
                else
                    package = null;
            }
            return Current = package != null ? Handler.PackageManager.GetMetaData(package) : null;
        }

        public void Remove() {
            Handler.PackageManager.DeletePackagesThatExist(ActualPackages);
        }

        public void UpdateLocalDependency(BaseVersion dependency) {
            CurrentLocalDependency = dependency == null
                ? _globalJoker
                : (LocalPackages.FirstOrDefault(x => x.Equals(dependency)) ?? _globalJoker);
        }
    }
}