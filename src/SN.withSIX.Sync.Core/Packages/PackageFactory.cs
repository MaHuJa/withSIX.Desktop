// <copyright company="SIX Networks GmbH" file="PackageFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages.Internals;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Core.Packages
{
    public class PackageFactory
    {
        public static string Packify(string input) {
            return input.ToLower();
        }

        public static string PackifyPath(string input) {
            return Packify(Path.GetFileNameWithoutExtension(input));
        }

        public static string GetPackageNameFromDirectory(string input) {
            return PackifyPath(Repository.RepoTools.GetRootedPath(input).ToString());
        }

        public Package Init(Repository repo, IAbsoluteDirectoryPath directory, PackageMetaData initialMetaData = null) {
            Contract.Requires<ArgumentNullException>(directory != null);

            if (initialMetaData == null)
                initialMetaData = new PackageMetaData(PackifyPath(directory.ToString()));

            if (string.IsNullOrWhiteSpace(initialMetaData.Name))
                throw new Exception("Initial metadata lacks Package Name");

            if (initialMetaData.Version == null)
                throw new Exception("Initial metadata lacks Version");

            var depName = initialMetaData.GetFullName();
            if (repo.HasPackage(depName))
                throw new Exception("Package and version already exists: " + depName);

            return new Package(directory, initialMetaData, repo);
        }

        public Package Init(Repository repo, string packageName, IAbsoluteDirectoryPath directory) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(packageName));
            return Init(repo, directory, new PackageMetaData(Packify(packageName)));
        }

        public Package Init(Repository repo, SpecificVersion version, IAbsoluteDirectoryPath directory)
        {
            Contract.Requires<ArgumentNullException>(version != null);
            return Init(repo, directory, new PackageMetaData(version));
        }

        public Package Import(Repository repo, IAbsoluteDirectoryPath workDir, IAbsoluteFilePath packageFilePath) {
            var metaData = Package.Load(packageFilePath);
            var package = new Package(workDir, metaData, repo);
            package.Commit(metaData.GetVersionInfo());

            return package;
        }

        public IAbsoluteDirectoryPath GetRepoPath(string repoDirectory, IAbsoluteDirectoryPath directory,
            bool local = false) {
            if (string.IsNullOrWhiteSpace(repoDirectory)) {
                repoDirectory = directory.GetChildDirectoryWithName(Repository.DefaultRepoRootDirectory).ToString();
                if (!Directory.Exists(repoDirectory)) {
                    var dir = Tools.FileUtil.FindPathInParents(directory.ToString(), Repository.DefaultRepoRootDirectory);
                    if (dir != null && Directory.Exists(dir))
                        repoDirectory = dir;
                    else if (local)
                        repoDirectory = directory.ToString();
                }
            }
            return Legacy.SixSync.Repository.RepoTools.GetRootedPath(repoDirectory);
        }

        public Package Open(Repository repo, IAbsoluteDirectoryPath directory, string packageName = null) {
            Contract.Requires<ArgumentNullException>(repo != null);
            Contract.Requires<ArgumentNullException>(directory != null);

            if (string.IsNullOrWhiteSpace(packageName))
                packageName = PackifyPath(directory.ToString());

            var match = BaseVersion.RxPackageName.Match(packageName);
            if (!match.Success) {
                var packDi = repo.PackagesPath.DirectoryInfo;
                var package = packDi.EnumerateFiles(packageName + "-*.json").LastOrDefault();
                if (package == null)
                    throw new PackageNotFoundException("No package found that matches " + packageName);
                packageName = Path.GetFileNameWithoutExtension(package.FullName);
            }

            return new Package(directory, packageName, repo);
        }
    }

    [DoNotObfuscate]
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string message) : base(message) {}
    }
}