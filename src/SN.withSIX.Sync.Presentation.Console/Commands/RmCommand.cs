// <copyright company="SIX Networks GmbH" file="RmCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using ManyConsole;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class RmCommand : BaseCommand
    {
        public bool IsBundle;
        public string PackageName;
        public string RepoDir;
        public string WorkDir;

        public RmCommand() {
            IsCommand("rm", "Remove Packages or Bundles");
            //HasOption("r|repodir=", "Specify repository directory", r => RepoDir = r);
            HasOption("p|package=", SynqStrings.PackageStr, p => PackageName = p);
            HasOption("b|bundle", "Bundle instead of Package", c => IsBundle = c != null);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            AllowsAnyAdditionalArguments("<packagename> [<packagename2> ...]");
        }

        public override int Run(params string[] remainingArguments) {
            var workDir = Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one package");

            var packages = remainingArguments;
            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, workDir))) {
                var pm = PackageManager.Create(repo, workDir).Result;
                if (IsBundle) {
                    pm.DeleteBundle(packages);
                    System.Console.WriteLine("Bundles:\n{0}", string.Join("\n", packages));
                } else {
                    pm.DeletePackages(packages);
                    System.Console.WriteLine("Packages:\n{0}", string.Join("\n", packages));
                }
            }

            return 0;
        }
    }
}