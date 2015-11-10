// <copyright company="SIX Networks GmbH" file="ListCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class ListCommand : BaseCommandAsync
    {
        public bool IsBundle;
        public string RepoDir;
        public string WorkDir;

        public ListCommand() {
            IsCommand("list", "List packages");
            HasOption("b|bundle", "Bundle instead of Package", c => IsBundle = c != null);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            //HasOption("remote", "List remote packages (local otherwise)", r => Remote = r);
            HasOption("remote=", "List remote packages from specified remote uuid (leave blank for all remotes)",
                r => Remote = r);
            AllowsAnyAdditionalArguments("<packagename> [<packagename2> ...]");
        }

        protected string Remote { get; set; }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            var path = Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, path))) {
                var pm = await PackageManager.Create(repo, path).ConfigureAwait(false);
                var list = await pm.List(Remote).ConfigureAwait(false);
                if (remainingArguments.Any())
                    list = list.Where(x => remainingArguments.Any(x.ContainsIgnoreCase)).ToArray();
                System.Console.WriteLine("Packages:\n{0}", string.Join("\n", list));
            }

            return 0;
        }
    }
}