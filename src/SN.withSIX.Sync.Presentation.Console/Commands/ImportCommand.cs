// <copyright company="SIX Networks GmbH" file="ImportCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class ImportCommand : BaseCommand
    {
        public string RepoDir;
        public string WorkDir;

        public ImportCommand() {
            IsCommand("import", "Import package to a repository");
            HasAdditionalArguments(1, "import path");
            HasOption("r|repodir=", SynqStrings.RepoDirStr, w => RepoDir = w);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
        }

        public override int Run(params string[] remainingArguments) {
            var path = Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, path)))
                Package.Factory.Import(repo, path, remainingArguments.First().ToAbsoluteFilePath());

            return 0;
        }
    }
}