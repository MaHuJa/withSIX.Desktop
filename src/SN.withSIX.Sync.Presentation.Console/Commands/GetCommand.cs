// <copyright company="SIX Networks GmbH" file="GetCommand.cs">
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
using SN.withSIX.Sync.Presentation.Console.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class GetCommand : BaseCommandAsync
    {
        public string Additional;
        public bool IncludeOptional;
        public bool IsBundle;
        public int? Limit;
        public bool NoCheckout;
        public string PackageName;
        public string Remote;
        public string RepoDir;
        public string Scope;
        public bool SkipWhenLocalMatch;
        public bool UpdateMetaData;
        public bool? UseVersionedPackageFolders;
        public string WorkDir;

        public GetCommand() {
            IsCommand("get", "Get a package or bundle (migrate the working directory)");
            HasOption("b|bundle", "Bundle instead of Package", c => IsBundle = c != null);
            HasOption("o|optional", "Include Optional Packages. Only relevant for Bundles",
                o => IncludeOptional = o != null);
            HasOption("a|additional=", "Include additional packages. Only relevant for Bundles", a => Additional = a);
            HasOption("s|scope=", "all, server or client. Only relevant for Bundles", s => Scope = s);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            HasOption<int?>("l|limit=", "Limit amount (removes older packages beyond this limit)", r => Limit = r);
            HasOption("v|versioned", SynqStrings.VersionedStr, r => {
                if (r != null)
                    UseVersionedPackageFolders = true;
            });
            HasOption("u|unversioned", SynqStrings.UnversionedStr, r => {
                if (r != null)
                    UseVersionedPackageFolders = false;
            });
            HasOption("remote=", SynqStrings.RemoteStr, r => Remote = r);
            HasOption("updateremotes", "Update remotes, or use cached metadata", u => UpdateMetaData = u != null);
            HasOption("skiplocalmatch", "Skip object fetch when local object matches already",
                s => SkipWhenLocalMatch = s != null);
            HasOption("n|nocheckout", "Does not checkout the packages so dependency conflicts are ignored",
                n => NoCheckout = n != null);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            AllowsAnyAdditionalArguments("<packagename> [<packagename2> ...]");
        }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            var workDir = Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            var packages = new[] {workDir.DirectoryName};
            if (remainingArguments.Any())
                packages = remainingArguments;

            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, workDir), true))
                await Process(workDir, repo, packages).ConfigureAwait(false);

            return 0;
        }

        Task Process(IAbsoluteDirectoryPath workDir, Repository repo, string[] packages) {
            return IsBundle ? GetBundles(workDir, repo, packages) : GetPackages(workDir, repo, packages);
        }

        async Task GetPackages(IAbsoluteDirectoryPath workDir, Repository repo, string[] packages) {
            var pm = await PackageManager.Create(repo, workDir, false, Remote).ConfigureAwait(false);
            if (UpdateMetaData) {
                System.Console.WriteLine("Updating remotes, please be patient...");
                await pm.UpdateRemotes().ConfigureAwait(false);
            }

            Package[] packs;
            System.Console.WriteLine("Querying Packages: {0}", String.Join(", ", packages));
            using (new ConsoleProgress(pm.StatusRepo)) {
                packs =
                    await
                        pm.ProcessPackages(packages.Select(x => new Dependency(x)), UseVersionedPackageFolders,
                            NoCheckout, SkipWhenLocalMatch)
                            .ConfigureAwait(false);
            }
            FinalProgress(packs);

            if (Limit.HasValue) {
                await
                    pm.CleanPackages(Limit.Value, packs.Select(x => x.MetaData.ToSpecificVersion()).ToArray(), packages)
                        .ConfigureAwait(false);
            }
        }

        async Task GetBundles(IAbsoluteDirectoryPath workDir, Repository repo, string[] packages) {
            var bundleManager = await BundleManager.Create(repo, workDir, false, Remote).ConfigureAwait(false);
            if (UpdateMetaData) {
                System.Console.WriteLine("Updating remotes, please be patient...");
                await bundleManager.PackageManager.UpdateRemotes().ConfigureAwait(false);
            }

            var scope = BundleScope.All;
            if (!string.IsNullOrWhiteSpace(Scope)) {
                if (!Enum.TryParse(Scope, true, out scope))
                    throw new ConsoleHelpAsException("Invalid scope: " + Scope);
            }
            var packs = new List<Package>();

            System.Console.WriteLine("Querying Bundles: {0}. Scope: {1}, IncludeOptional: {2}. Additional: {3}",
                String.Join(", ", packages), scope, IncludeOptional, Additional);

            using (new ConsoleProgress(bundleManager.PackageManager.StatusRepo)) {
                packs.AddRange(
                    await bundleManager.Process(new Bundle("selected") {
                        Dependencies =
                            packages.Select(x => new Dependency(x))
                                .ToDictionary(x => x.Name, x => x.GetConstraints()),
                        Required =
                            Additional == null
                                ? new Dictionary<string, string>()
                                : Additional.Split(',')
                                    .Select(x => new Dependency(x))
                                    .ToDictionary(x => x.Name, x => x.GetConstraints())
                    }, IncludeOptional, scope, UseVersionedPackageFolders, NoCheckout, SkipWhenLocalMatch)
                        .ConfigureAwait(false));
            }
            FinalProgress(packs);
        }

        static void FinalProgress(IEnumerable<Package> packs) {
            System.Console.WriteLine("\nSuccesfully got {0}",
                string.Join(", ", packs.Select(x => x.MetaData.GetFullName())));
        }
    }
}