// <copyright company="SIX Networks GmbH" file="CommitCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Repositories;
using SN.withSIX.Sync.Core.Services;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class CommitCommand : BaseCommandAsync
    {
        readonly IPublishingApi _publishingApi;
        SynqConfig _config;
        public bool Create;
        public int? Limit;
        public bool NoDownCase;
        public string Package;
        string Register;
        public string RepoDir;
        public string Version;

        public CommitCommand(IPublishingApi publishingApi) {
            _publishingApi = publishingApi;
            IsCommand("commit", "Commit changes to a repository");
            HasOption("f|force", "Create new package version even if unchanged", f => Force = f != null);
            HasOption("c|create", "Create new package", c => Create = c != null);
            HasOption<int?>("l|limit=", "Limit amount (removes older packages beyond this limit)", r => Limit = r);
            HasOption("n|nodowncase",
                "Do not downcase the relative file path keys. Not recommended due to Windows vs POSIX clients",
                n => NoDownCase = n != null);
            HasOption("r|repodir=", SynqStrings.RepoDirStr, w => RepoDir = w);
            HasOption("p|package=", SynqStrings.PackageStr + "\nRequired if name is not equal to Current folder name",
                w => Package = w);
            HasOption("v|version=", "Desired version", v => Version = v);
            HasOption("register=", "Reserved", r => Register = r);
        }

        protected bool Force { get; set; }

        protected override async Task<int> RunAsync(params string[] remainingArguments) {
            _config = GetConfig();

            HandleDefaultVersion();
            var path = Repository.RepoTools.GetRootedPath(".");
            using (var repo = GetRepo(Core.Packages.Package.GetRepoPath(RepoDir, path))) {
                var package = TryOpenOrInitPackage(repo, path);
                if (await Commit(package).ConfigureAwait(false))
                    System.Console.WriteLine("Finished comitting {0}", package.MetaData.GetFullName());
                else
                    System.Console.WriteLine("Did not find any changes, use --force to force");

                await ProcessLimit(repo, package).ConfigureAwait(false);
            }

            return 0;
        }

        async Task ProcessLimit(Repository repo, Package package) {
            if (!Limit.HasValue)
                return;

            var pm = await PackageManager.Create(repo, package.WorkingPath).ConfigureAwait(false);
            await pm.CleanPackages(Limit.Value, null, package.MetaData.Name).ConfigureAwait(false);
        }

        void HandleDefaultVersion() {
            if (Version.IsBlank() && Create)
                Version = SpecificVersion.DefaultVersion;
        }

        async Task<bool> Commit(Package package) {
            var done = Version.IsBlank()
                ? package.Commit(Force, !NoDownCase)
                : package.Commit(Version, Force, !NoDownCase);
            if (done && Register != null && _config.RegisterKey != null) {
                System.Console.WriteLine("Registering with the API... please stand by");
                var registeredId =
                    await package.Register(_publishingApi, Register, _config.RegisterKey).ConfigureAwait(false);
                var shortId = new ShortGuid(registeredId);
                System.Console.WriteLine("The ID of the registration is: " + shortId);
            }
            return done;
        }

        Package TryOpenOrInitPackage(Repository repo, IAbsoluteDirectoryPath directory) {
            return Create
                ? InitPackage(repo, directory)
                : Core.Packages.Package.Factory.Open(repo, directory, Package);
        }

        Package InitPackage(Repository repo, IAbsoluteDirectoryPath directory) {
            if (Package == null)
                Package = PackageFactory.GetPackageNameFromDirectory(directory.ToString());

            return Core.Packages.Package.Factory.Init(repo, GetVersionInfo().GetFullName(), directory);
        }

        SpecificVersion GetVersionInfo() {
            return Version.IsBlank() ? new SpecificVersion(Package) : new SpecificVersion(Package, Version);
        }
    }
}