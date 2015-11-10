// <copyright company="SIX Networks GmbH" file="RepositoryFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy.SixSync
{
    public class RepositoryFactory : IEnableLogging
    {
        readonly ZsyncMake _zsyncMake;

        public RepositoryFactory(ZsyncMake zsyncMake) {
            _zsyncMake = zsyncMake;
        }

        public Repository Init(IAbsoluteDirectoryPath folder, IReadOnlyCollection<Uri> hosts,
            Dictionary<string, object> opts = null) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(hosts != null);

            if (opts == null)
                opts = new Dictionary<string, object>();

            var rsyncFolder = folder.GetChildDirectoryWithName(Repository.RepoFolderName);
            if (rsyncFolder.Exists)
                throw new Exception("Already appears to be a repository");

            var packFolder = Path.Combine(opts.ContainsKey("pack_path")
                ? ((string) opts["pack_path"])
                : rsyncFolder.ToString(),
                Repository.PackFolderName).ToAbsoluteDirectoryPath();

            var configFile = rsyncFolder.GetChildFileWithName(Repository.ConfigFileName);
            var wdVersionFile = rsyncFolder.GetChildFileWithName(Repository.VersionFileName);
            var packVersionFile = packFolder.GetChildFileWithName(Repository.VersionFileName);

            this.Logger().Info("Initializing {0}", folder);
            rsyncFolder.MakeSurePathExists();
            packFolder.MakeSurePathExists();

            var config = new RepoConfig {Hosts = hosts.ToArray()};

            if (opts.ContainsKey("pack_path"))
                config.PackPath = (string) opts["pack_path"];

            if (opts.ContainsKey("include"))
                config.Include = (string[]) opts["include"];

            if (opts.ContainsKey("exclude"))
                config.Exclude = (string[]) opts["exclude"];

            var guid = opts.ContainsKey("required_guid")
                ? (string) opts["required_guid"]
                : Guid.NewGuid().ToString();

            var packVersion = new RepoVersion {Guid = guid};
            if (opts.ContainsKey("archive_format"))
                packVersion.ArchiveFormat = (string) opts["archive_format"];

            var wdVersion = YamlExtensions.NewFromYaml<RepoVersion>(packVersion.ToYaml());

            config.SaveYaml(configFile);
            packVersion.SaveYaml(packVersionFile);
            wdVersion.SaveYaml(wdVersionFile);

            return TryGetRepository(folder.ToString(), opts, rsyncFolder.ToString());
        }

        Repository TryGetRepository(string folder, Dictionary<string, object> opts, string rsyncFolder) {
            try {
                var repo = GetRepository(folder, opts);

                if (opts.ContainsKey("max_threads"))
                    repo.MultiThreadingSettings.MaxThreads = (int) opts["max_threads"];

                if (opts.ContainsKey("required_version"))
                    repo.RequiredVersion = (long?) opts["required_version"];

                if (opts.ContainsKey("required_guid"))
                    repo.RequiredGuid = (string) opts["required_guid"];

                if (opts.ContainsKey("output"))
                    repo.Output = (string) opts["output"];

                repo.LoadHosts();
                return repo;
            } catch (Exception) {
                Tools.FileUtil.Ops.DeleteWithRetry(rsyncFolder);
                throw;
            }
        }

        Repository GetRepository(string folder, IDictionary<string, object> opts) {
            var repo = opts.ContainsKey("status")
                ? new Repository(_zsyncMake, (StatusRepo) opts["status"], folder)
                : new Repository(_zsyncMake, folder);
            return repo;
        }

        public async Task<Repository> Clone(Uri[] hosts, string folder, Dictionary<string, object> opts = null) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(folder));
            Contract.Requires<ArgumentNullException>(hosts != null);

            if (opts == null)
                opts = new Dictionary<string, object>();
            if (opts.ContainsKey("path"))
                folder = Path.Combine((string) opts["path"], folder);
            var repo = Init(folder.ToAbsoluteDirectoryPath(), hosts, opts);
            await repo.Update(opts);
            return repo;
        }

        public Repository Open(string folder, Dictionary<string, object> opts = null) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(folder));

            if (opts == null)
                opts = new Dictionary<string, object>();

            var repo = GetRepository(folder, opts);
            if (opts.ContainsKey("output"))
                repo.Output = (string) opts["output"];

            return repo;
        }

        public Repository Convert(string folder, Dictionary<string, object> opts = null) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(folder));

            if (opts == null)
                opts = new Dictionary<string, object>();

            var hosts = opts.ContainsKey("hosts") && opts["hosts"] != null ? (Uri[]) opts["hosts"] : new Uri[0];
            var repo = Init(folder.ToAbsoluteDirectoryPath(), hosts, opts);
            repo.Commit(false, false);
            return repo;
        }
    }
}