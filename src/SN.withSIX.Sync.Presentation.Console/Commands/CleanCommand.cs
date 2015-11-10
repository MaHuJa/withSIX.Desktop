// <copyright company="SIX Networks GmbH" file="CleanCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Presentation.Console.UseCases;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class CleanCommand : BaseCommandAsync
    {
        readonly IMediator _mediator;
        public bool All;
        public int? Limit;
        public IDirectoryPath RepoDir;
        public string WorkDir;

        public CleanCommand(IMediator mediator) {
            _mediator = mediator;

            IsCommand("clean", "Cleanup packages");
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r == null ? null : r.ToDirectoryPath());
            HasOption<int?>("l|limit=", "Limit amount (default 1)", r => Limit = r.HasValue ? r.Value : 1);
            //HasOption("b|bundle", "Bundle instead of Package", c => IsBundle = c != null);
            HasOption("a|all", "All", r => All = r != null);
            AllowsAnyAdditionalArguments("<packagename> [<packagename2> ...]");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments) {
            var workDir = (Repository.RepoTools.GetRootedPath(WorkDir ?? "."));
            var failedOnce = false;
            var packages = new string[0];
            if (!All)
                packages = remainingArguments.Any() ? remainingArguments : new[] {workDir.DirectoryName};

            var cleanedPackages = (await _mediator.RequestAsync(
                new CleanPackageCommand(workDir,
                    packages) {
                        RepoDir = RepoDir,
                        Limit = Limit,
                        RetryAction = () => {
                            if (failedOnce)
                                return;
                            System.Console.WriteLine("Repository seems locked, retrying indefinitely....");
                            failedOnce = true;
                        },
                        All = All
                    }).ConfigureAwait(false));

            if (cleanedPackages.Any())
                System.Console.WriteLine("Cleaned the following packages: " + String.Join(", ", cleanedPackages));

            return 0;
        }
    }
}