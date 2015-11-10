// <copyright company="SIX Networks GmbH" file="RepositoryRemote.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Repositories.Internals
{
    public class RepositoryRemote
    {
        static readonly KeyValuePair<string, Func<IAbsoluteFilePath, bool>>[] defaultFiles = {
            new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(Repository.ConfigFile,
                Repository.ConfirmValidity<RepositoryConfigDto>),
            new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(Repository.PackageIndexFile,
                Repository.ConfirmValidity<RepositoryStorePackagesDto>),
            new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(Repository.CollectionIndexFile,
                Repository.ConfirmValidity<RepositoryStoreBundlesDto>)
        };

        public RepositoryRemote() {
            Config = new RepositoryConfig();
            Index = new RepositoryStore();
        }

        public RepositoryRemote(Uri[] uri) : this() {
            Urls = uri;
        }

        public RepositoryRemote(Uri[] uri, Guid id) : this(uri) {
            Config.Uuid = id;
        }

        public RepositoryRemote(Uri uri)
            : this() {
            Urls = new[] {uri};
        }

        public RepositoryRemote(Uri uri, Guid id)
            : this(uri) {
            Config.Uuid = id;
        }

        public RepositoryConfig Config { get; private set; }
        Uri[] Urls { get; }
        public RepositoryStore Index { get; }
        public IAbsoluteDirectoryPath Path { get; set; }

        public IEnumerable<Uri> GetRemotes() {
            if (Config.Remotes == null || !Config.Remotes.Any())
                return Urls.Distinct();
            lock (Config.Remotes) {
                return Config.Remotes.Values
                    .SelectMany(x => x).Distinct();
            }
        }

        public Task LoadAsync(bool includeObjects = false) {
            return Task.Run(() => Load(includeObjects));
        }

        public void Load(bool includeObjects = false) {
            LoadConfig();
            LoadPackages();
            LoadBundles();
            if (includeObjects)
                LoadObjects();
        }

        void LoadConfig() {
            Path.MakeSurePathExists();
            Config =
                Repository.TryLoad<RepositoryConfigDto, RepositoryConfig>(
                    Path.GetChildFileWithName(Repository.ConfigFile));
        }

        void LoadPackages() {
            Path.MakeSurePathExists();
            var index =
                Repository.TryLoad<RepositoryStorePackagesDto>(Path.GetChildFileWithName(Repository.PackageIndexFile));
            Index.Packages = index.Packages.ToDictionary(x => x.Key, x => x.Value);
            Index.PackagesContentTypes = index.PackagesContentTypes.ToDictionary(x => x.Key, x => x.Value);
        }

        void LoadBundles() {
            Path.MakeSurePathExists();
            Index.Bundles =
                Repository.TryLoad<RepositoryStoreBundlesDto>(Path.GetChildFileWithName(Repository.CollectionIndexFile))
                    .Bundles.ToDictionary(x => x.Key, x => x.Value);
        }

        void LoadObjects() {
            Path.MakeSurePathExists();
            Index.Objects =
                Repository.TryLoad<RepositoryStoreObjectsDto>(Path.GetChildFileWithName(Repository.ObjectIndexFile))
                    .Objects;
        }

        public async Task Update(bool inclObjects = false) {
            await
                DownloadFiles(inclObjects
                    ? defaultFiles.Concat(new[] {
                        new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(Repository.ObjectIndexFile,
                            Repository.ConfirmValidity<RepositoryStoreObjectsDto>)
                    })
                    : defaultFiles)
                    .ConfigureAwait(false);

            await LoadAsync(inclObjects).ConfigureAwait(false);
        }

        public static int CalculateHttpFallbackAfter(int limit) {
            return Math.Max((int) (limit/3.0), 1);
        }

        async Task DownloadFiles(IEnumerable<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>> remoteFiles) {
            var retryLimit = 6;
            using (var statusRepo = new StatusRepo()) {
                await SyncEvilGlobal.DownloadHelper.DownloadFilesAsync(Urls, statusRepo,
                    remoteFiles.ToDictionary(x => x,
                        x =>
                            (ITransferStatus)
                                new TransferStatus(x.Key) {
                                    ZsyncHttpFallbackAfter = CalculateHttpFallbackAfter(retryLimit)
                                }), Path,
                    retryLimit).ConfigureAwait(false);
            }
        }
    }
}