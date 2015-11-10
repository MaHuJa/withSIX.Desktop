// <copyright company="SIX Networks GmbH" file="YankCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class YankCommand : BaseCommandAsync
    {
        readonly IPublishingApi _publishingApi;
        SynqConfig _config;
        public bool Register;
        public string RepoDir;
        public string WorkDir;

        public YankCommand(IPublishingApi publishingApi) {
            _publishingApi = publishingApi;
            IsCommand("yank", "Yank package");
            HasOption("register", "Register with API", r => Register = r != null);
            HasOption("w|workdir=", SynqStrings.WorkDirStr, r => WorkDir = r);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, r => RepoDir = r);
            AllowsAnyAdditionalArguments("<package> (<package>...)");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments) {
            var workDir = Core.Legacy.SixSync.Repository.RepoTools.GetRootedPath(WorkDir ?? ".");
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one package");

            _config = GetConfig();
            if (Register) {
                if (_config.RegisterKey == null)
                    throw new Exception("No key registered");
            }

            using (var repo = GetRepo(Package.Factory.GetRepoPath(RepoDir, workDir))) {
                foreach (var p in remainingArguments)
                    await Yank(repo, new Dependency(p), workDir).ConfigureAwait(false);
            }

            return 0;
        }

        Task Yank(Repository repo, Dependency p, IAbsoluteDirectoryPath workDir) {
            return Register
                ? repo.YankAndDeregister(p, workDir, _publishingApi, _config.RegisterKey)
                : repo.Yank(p, workDir);
        }
    }
}