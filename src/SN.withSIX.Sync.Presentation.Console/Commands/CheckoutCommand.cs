// <copyright company="SIX Networks GmbH" file="CheckoutCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Repositories.Internals;
using SN.withSIX.Sync.Presentation.Console.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class CheckoutCommand : BaseCommandAsync
    {
        public string Additional;
        public bool IncludeOptional;
        public bool IsBundle;
        public string PackageName;
        public string RepoDir;
        public string Scope;
        public bool? UseVersionedPackageFolders;
        public string WorkDir;

        public CheckoutCommand() {
            IsCommand("checkout", "Checkout a package (migrate the working directory)");
            HasOption("b|bundle", "Bundle instead of Package", c => IsBundle = c != null);
            HasOption("o|optional", "Include Optional Packages. Only relevant for Bundles",
                o => IncludeOptional = o != null);
            HasOption("a|additional=", "Include additional packages. Only relevant for Bundles", a => Additional = a);
            HasOption("s|scope=", "all, server or client. Only relevant for Bundles", s => Scope = s);

            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            HasOption("v|versioned", SynqStrings.VersionedStr, r => {
                if (r != null)
                    UseVersionedPackageFolders = true;
            });
            HasOption("u|unversioned", SynqStrings.UnversionedStr, r => {
                if (r != null)
                    UseVersionedPackageFolders = false;
            });
            AllowsAnyAdditionalArguments("<packagename> [<packagename2> ...]");
        }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            var path = Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            var packages = new[] {path.DirectoryName};
            if (remainingArguments.Any())
                packages = remainingArguments;

            using (var repo = GetRepo(Repository.RepoTools.GetRootedPath(RepoDir ?? "."))) {
                if (IsBundle)
                    await CheckoutBundles(path, repo, packages).ConfigureAwait(false);
                else
                    await CheckoutPackages(path, repo, packages).ConfigureAwait(false);
            }

            return 0;
        }

        async Task CheckoutPackages(IAbsoluteDirectoryPath path, Repository repo, string[] packages) {
            var pm = await PackageManager.Create(repo, path).ConfigureAwait(false);
            ConfirmOperationMode(pm.Repo, packages.Length);

            Package[] packs;
            using (new ConsoleProgress(pm.StatusRepo))
                packs = pm.Checkout(packages, UseVersionedPackageFolders);
            System.Console.WriteLine("\nSuccesfully checked out {0} packages",
                string.Join(", ", packs.Count()));
        }

        async Task CheckoutBundles(IAbsoluteDirectoryPath path, Repository repo, string[] packages) {
            var pm = await BundleManager.Create(repo, path).ConfigureAwait(false);
            ConfirmOperationMode(pm.Repo, 2);

            var scope = BundleScope.All;
            if (!string.IsNullOrWhiteSpace(Scope)) {
                if (!Enum.TryParse(Scope, true, out scope))
                    throw new ConsoleHelpAsException("Invalid scope: " + Scope);
            }

            System.Console.WriteLine("Querying Bundles: {0}. Scope: {1}, IncludeOptional: {2}. Additional: {3}",
                String.Join(", ", packages), scope, IncludeOptional, Additional);

            Package[] packs;
            using (new ConsoleProgress(pm.PackageManager.StatusRepo)) {
                packs = await pm.Checkout(new Bundle("selected") {
                    Dependencies =
                        packages.Select(x => new Dependency(x))
                            .ToDictionary(x => x.Name, x => x.GetConstraints()),
                    Required =
                        Additional == null
                            ? new Dictionary<string, string>()
                            : Additional.Split(',')
                                .Select(x => new Dependency(x))
                                .ToDictionary(x => x.Name, x => x.GetConstraints())
                }, IncludeOptional, scope, UseVersionedPackageFolders).ConfigureAwait(false);
            }

            System.Console.WriteLine("\nSuccesfully checked out {0} packages",
                string.Join(", ", packs.Count()));
        }

        static void ConfirmOperationMode(Repository repo, int count = 1) {
            if (repo.Config.OperationMode == RepositoryOperationMode.SinglePackage
                && count > 1)
                throw new Exception("Not a MultiPackage repository or working folder");
        }
    }
}