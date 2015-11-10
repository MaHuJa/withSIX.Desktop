// <copyright company="SIX Networks GmbH" file="CleanPackageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Sync.Presentation.Console.UseCases
{
    public class CleanPackageCommand : IAsyncRequest<IReadOnlyCollection<string>>
    {
        public CleanPackageCommand(IAbsoluteDirectoryPath workDir, params string[] packages) {
            Contract.Requires<ArgumentNullException>(workDir != null);
            Contract.Requires<ArgumentNullException>(packages != null);
            WorkDir = workDir;
            Packages = packages;
        }

        public string[] Packages { get; }
        public IAbsoluteDirectoryPath WorkDir { get; }
        public IDirectoryPath RepoDir { get; set; }
        public Action RetryAction { get; set; }
        public int? Limit { get; set; }
        public bool All { get; set; }
    }

    [StayPublic]
    public class CleanPackageCommandHandler : IAsyncRequestHandler<CleanPackageCommand, IReadOnlyCollection<string>>
    {
        public async Task<IReadOnlyCollection<string>> HandleAsync(CleanPackageCommand request) {
            using (
                var repo =
                    Repository.Factory.OpenRepositoryWithRetry(
                        Package.Factory.GetRepoPath(request.RepoDir.ToString(), request.WorkDir), true,
                        request.RetryAction)
                ) {
                var pm = await PackageManager.Create(repo, request.WorkDir).ConfigureAwait(false);
                var requestPackages = request.Packages;
                if (request.All)
                    requestPackages = pm.Repo.GetPackages().Select(x => x.Key).ToArray();

                var task = request.Limit.HasValue
                    ? pm.CleanPackages(request.Limit.Value, null, requestPackages)
                    : pm.CleanPackages(null, requestPackages);
                var packages = await task.ConfigureAwait(false);
                return packages.Select(x => x.GetFullName()).ToArray();
            }
        }
    }
}