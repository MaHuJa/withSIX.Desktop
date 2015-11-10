// <copyright company="SIX Networks GmbH" file="UtilsCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class UtilsCommand : BaseCommandAsync
    {
        public string RepoDir;
        public string WorkDir;

        public UtilsCommand() {
            IsCommand("utils", "Utils");
            HasAdditionalArguments(1, "util");
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
        }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            using (
                var repo =
                    GetRepo(Package.Factory.GetRepoPath(RepoDir,
                        Repository.RepoTools.GetRootedPath(WorkDir ?? ".")))) {
                switch (remainingArguments[0]) {
                case "repair": {
                    await repo.Repair().ConfigureAwait(false);
                    break;
                }
                }
            }

            return 0;
        }
    }
}