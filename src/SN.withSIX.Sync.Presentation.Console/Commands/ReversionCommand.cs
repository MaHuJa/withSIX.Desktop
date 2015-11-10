// <copyright company="SIX Networks GmbH" file="ReversionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class ReversionCommand : BaseCommandAsync
    {
        readonly IPublishingApi _publishingApi;
        SynqConfig _config;
        public bool Register;
        public string RepoDir;
        public string WorkDir;

        public ReversionCommand(IPublishingApi publishingApi) {
            _publishingApi = publishingApi;
            IsCommand("reversion", "Reversion package");
            HasOption("register", "Register with API", r => Register = r != null);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            HasAdditionalArguments(2, "<package> <newversion>");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments) {
            var workDir = Core.Legacy.SixSync.Repository.RepoTools.GetRootedPath(WorkDir ?? ".");

            var package = remainingArguments[0];
            var newVersion = remainingArguments[1];

            _config = GetConfig();
            if (Register) {
                if (_config.RegisterKey == null)
                    throw new Exception("No key registered");
            }

            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, workDir)))
                await Reversion(repo, new Dependency(package), workDir, newVersion).ConfigureAwait(false);

            return 0;
        }

        Task Reversion(Repository repo, Dependency p, IAbsoluteDirectoryPath workDir, string newVersion) {
            return Register
                ? repo.ReversionAndRegister(p, newVersion, workDir, _publishingApi, _config.RegisterKey)
                : repo.Reversion(p, workDir, newVersion);
        }
    }
}