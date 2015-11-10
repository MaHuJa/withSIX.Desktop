// <copyright company="SIX Networks GmbH" file="InitCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using ManyConsole;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class InitCommand : BaseCommandAsync
    {
        public bool Bare;
        public string PackageName;
        public string Remote;
        public string RepoDir;

        public InitCommand() {
            IsCommand("init", "Initialize a repository");
            HasOption("bare", "Initialize a bare repository (without working directory)", b => Bare = true);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            HasOption("remote=", SynqStrings.RemoteStr, r => Remote = r);
            HasOption("p|package=", SynqStrings.PackageStr, p => PackageName = p);
            AllowsAnyAdditionalArguments("[<Directory> defaults to current]");
        }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            var path = Repository.RepoTools.GetRootedPath(remainingArguments.FirstOrDefault() ?? ".");
            if (Bare) {
                if (!string.IsNullOrWhiteSpace(PackageName))
                    throw new ConsoleHelpAsException("Cannot specify package with bare repo");
                if (!string.IsNullOrWhiteSpace(RepoDir)) {
                    throw new ConsoleHelpAsException(
                        "Cannot specify repodir with bare repo, use <TargetDirectory> argument instead");
                }
                using (Repository.Factory.Init(path)) {}
                System.Console.WriteLine("Initialized Bare repository at: {0}", path);
            } else {
                using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, path), true)) {
                    if (string.IsNullOrWhiteSpace(PackageName)) {
                        if (!string.IsNullOrWhiteSpace(Remote)) {
                            var pm = await PackageManager.Create(repo, path, true, Remote).ConfigureAwait(false);
                            System.Console.WriteLine(
                                "Initialized Working directory at: {0}, repository at: {1}, with Remote: {2}",
                                pm.WorkDir, pm.Repo.RootPath, Remote);
                            System.Console.WriteLine("Updating remotes, please be patient...");
                            if (Remote != null)
                                await pm.UpdateRemotes().ConfigureAwait(false);
                        } else {
                            var p = Package.Factory.Init(repo, path, null);
                            System.Console.WriteLine("Initialized Package: {0}", p.MetaData.GetFullName());
                        }
                    } else {
                        var p = Package.Factory.Init(repo, PackageName, path);
                        System.Console.WriteLine("Initialized Package: {0}", p.MetaData.GetFullName());
                    }
                }
            }
            return 0;
        }
    }
}