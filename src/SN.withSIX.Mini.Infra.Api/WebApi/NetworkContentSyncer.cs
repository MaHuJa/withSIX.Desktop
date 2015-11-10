// <copyright company="SIX Networks GmbH" file="NetworkContentSyncer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Sync.Core;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;
using YamlDotNet.Core;

namespace SN.withSIX.Mini.Infra.Api.WebApi
{
    public class NetworkContentSyncer : IInfrastructureService, INetworkContentSyncer
    {
        readonly IDbContextLocator _locator;

        public NetworkContentSyncer(IDbContextLocator locator) {
            _locator = locator;
        }

        public Task<ApiHashes> GetHashes() {
            return Tools.Transfer.GetJson<ApiHashes>(new Uri("http://api-cdn.withsix.com/api/v2/hashes.json.gz"));
        }

        public async Task SyncContent(IReadOnlyCollection<Game> games, ApiHashes hashes) {
            var allNetworkContent = await DownloadContentLists(games.Select(x => x.Id), hashes).ConfigureAwait(false);
            foreach (var g in games)
                ProcessGame(g, allNetworkContent);
        }

        public async Task SyncCollections(IReadOnlyCollection<SubscribedCollection> collections,
            IReadOnlyCollection<NetworkContent> content, bool countCheck = true) {
            var contents =
                await
                    DownloadCollections(
                        collections.GroupBy(x => x.GameId)
                            .Select(x => new Tuple<Guid, List<Guid>>(x.Key, x.Select(t => t.Id).ToList())))
                        .ConfigureAwait(false);

            if (countCheck && contents.Count < collections.Count)
                throw new Exception("Could not find all requested collections");

            foreach (var c in contents) {
                var col = collections.Find(c.Id);
                c.MapTo(col);
                HandleContent(content, col, c, await GetRepositories(col).ConfigureAwait(false));
            }
        }

        public async Task<IReadOnlyCollection<SubscribedCollection>> GetCollections(Guid gameId,
            IReadOnlyCollection<Guid> collectionIds, IReadOnlyCollection<NetworkContent> content) {
            var contents =
                await
                    DownloadCollections(new[] {Tuple.Create(gameId, collectionIds.ToList())})
                        .ConfigureAwait(false);
            if (contents.Count < collectionIds.Count)
                throw new Exception("Could not find all requested collections");
            var collections = new List<SubscribedCollection>();
            foreach (var c in contents) {
                var col = c.MapTo<SubscribedCollection>();
                HandleContent(content, col, c, await GetRepositories(col).ConfigureAwait(false));
                collections.Add(col);
            }
            return collections;
        }

        static async Task<CustomRepo[]> GetRepositories(SubscribedCollection col) {
            var repositories = col.Repositories.Select(r => new CustomRepo(CustomRepo.GetRepoUri(new Uri(r)))).ToArray();
            foreach (var r in repositories)
                await r.Load(SyncEvilGlobal.StringDownloader).ConfigureAwait(false);

            return repositories;
        }

        async Task<List<ModDto>> DownloadContentLists(IEnumerable<Guid> gameIds, ApiHashes hashes) {
            var mods =
                await
                    Tools.Transfer.GetJson<List<ModDto>>(
                        new Uri("http://api-cdn.withsix.com/api/v2/mods.json.gz?v=" + hashes.Mods))
                        .ConfigureAwait(false);
            return mods.Where(x => gameIds.Contains(x.GameId)).ToList();
        }

        static void ProcessGame(Game game, IEnumerable<ModDto> allContent) {
            var compatGameIds = game.GetCompatibleGameIds();

            var gameContent = allContent.Where(x => compatGameIds.Contains(x.GameId)).ToArray();
            ProcessContents(game, gameContent);
            ProcessLocalContent(game);
        }

        static void ProcessContents(Game game, IEnumerable<ModDto> contents) {
            // TODO: If we timestamp the DTO's, and save the timestamp also in our database,
            // then we can simply update data only when it has actually changed and speed things up.
            // The only thing to remember is when there are schema changes / new fields etc, either all timestamps need updating
            // or the syncer needs to take it into account..
            var mapping = new Dictionary<ModDto, NetworkContent>();
            UpdateContents(game, contents, mapping);
            HandleDependencies(game, mapping);
        }

        static void UpdateContents(Game game, IEnumerable<ModDto> contents, IDictionary<ModDto, NetworkContent> content) {
            var newContent = new List<NetworkContent>();
            foreach (
                var c in
                    contents.Where(x => x.GameId == game.Id)
                        .Select(x => new {DTO = x, Existing = game.NetworkContent.Find(x.Id)})) {
                if (c.Existing == null) {
                    var nc = c.DTO.MapTo<NetworkContent>();
                    newContent.Add(nc);
                    content[c.DTO] = nc;
                } else {
                    content[c.DTO] = c.Existing;
                    c.DTO.MapTo(c.Existing);
                }
            }
            game.Contents.AddRange(newContent);
        }

        static void HandleDependencies(Game game, Dictionary<ModDto, NetworkContent> content) {
            foreach (var nc in content)
                HandleDependencies(nc, game.NetworkContent);
        }

        // TODO: catch frigging circular reference mayhem!
        // http://stackoverflow.com/questions/16472958/json-net-silently-ignores-circular-references-and-sets-arbitrary-links-in-the-ch
        // http://stackoverflow.com/questions/21686499/how-to-restore-circular-references-e-g-id-from-json-net-serialized-json
        static void HandleDependencies(KeyValuePair<ModDto, NetworkContent> nc,
            IEnumerable<NetworkContent> networkContent) {
            nc.Value.Dependencies.Replace(
                nc.Key.Dependencies.Select(
                    d =>
                        networkContent.FirstOrDefault(
                            x => x.PackageName.Equals(d, StringComparison.CurrentCultureIgnoreCase)))
                    // TODO: Find out why we would have nulls..
                    .Where(x => x != null)
                    .Select(x => new NetworkContentSpec(x)));
        }

        static void ProcessLocalContent(Game game) {
            var networkContents = game.NetworkContent.ToArray();
            foreach (var c in game.LocalContent)
                ProcessLocalContent(c, networkContents);
        }

        static void ProcessLocalContent(LocalContent localContent, IReadOnlyCollection<NetworkContent> contents) {
            var nc = contents.FirstOrDefault(x => x.PackageName.Equals(localContent.PackageName)) ??
                     contents.Find(localContent.ContentId);
            if (nc == null)
                return;
            localContent.UpdateFrom(nc);
        }

        static void HandleContent(IReadOnlyCollection<NetworkContent> content, Collection col,
            CollectionModelWithLatestVersion c, CustomRepo[] customRepos) {
            col.Contents.Replace(
                c.LatestVersion
                    .Dependencies
                    .Select(
                        x =>
                            new {
                                Content = ConvertToRepoContent(x, col, customRepos, content) ??
                                          ConvertToContentOrLocal(x, col, content), // temporary
                                x.Constraint
                            })
                    .Where(x => x.Content != null)
                    .Select(x => new ContentSpec(x.Content, x.Constraint))
                    .ToList());
        }

        static Content ConvertToRepoContent(CollectionVersionDependencyModel x, Collection col, CustomRepo[] customRepos, IReadOnlyCollection<NetworkContent> content) {
            var repo = customRepos.FirstOrDefault(r => r.HasMod(x.Dependency));
            if (repo == null)
                return null;
            var repoContent = repo.GetMod(x.Dependency);
            var mod = new ModRepoContent(x.Dependency, x.Dependency, col.GameId);
            if (repoContent.Value.Dependencies != null)
                mod.Dependencies = GetDependencyTree(repoContent, customRepos, content);
            return mod;
        }

        static List<string> GetDependencyTree(KeyValuePair<string, SixRepoModDto> repoContent, CustomRepo[] customRepos, IReadOnlyCollection<NetworkContent> content) {
            var dependencies = new List<string>();
            var name = repoContent.Key.ToLower();
            // TODO: Would be better to build the dependency tree from actual objects instead of strings??
            BuildDependencyTree(dependencies, repoContent, customRepos, content);
            dependencies.Remove(name); // we dont want ourselves to be a dep of ourselves
            return dependencies;
        }

        static void BuildDependencyTree(List<string> dependencies, KeyValuePair<string, SixRepoModDto> repoContent, CustomRepo[] customRepos, IReadOnlyCollection<NetworkContent> content) {
            var name = repoContent.Key.ToLower();
            if (dependencies.Contains(name))
                return;
            dependencies.Add(name);
            if (repoContent.Value.Dependencies == null)
                return;

            foreach (var d in repoContent.Value.Dependencies) {
                var n = d.ToLower();
                var repo = customRepos.FirstOrDefault(r => r.HasMod(d));
                if (repo == null) {
                    var nc = content.FirstOrDefault(x => x.PackageName.Equals(d, StringComparison.InvariantCultureIgnoreCase));
                    if (nc != null) {
                        var deps = nc.GetRelatedContent().Select(x => x.Content).OfType<IHavePackageName>().Select(x => x.PackageName).Where(x => !dependencies.ContainsIgnoreCase(x)).ToArray();
                        // TODO: this does not take care of dependencies that actually exist then on the custom repo, and might have different deps setup than the official network counter parts..
                        // But the use case is very limited..
                        dependencies.AddRange(deps);
                    } else
                        dependencies.Add(n);
                }  else
                    BuildDependencyTree(dependencies, repo.GetMod(d), customRepos, content);
            }

            dependencies.Remove(name);
            dependencies.Add(name);
        }

        static Content ConvertToContentOrLocal(CollectionVersionDependencyModel x, Collection col,
            IEnumerable<NetworkContent> content) {
            return (Content) content.FirstOrDefault(
                cnt =>
                    cnt.PackageName.Equals(x.Dependency,
                        StringComparison.CurrentCultureIgnoreCase))
                   ?? new ModLocalContent(x.Dependency, x.Dependency, col.GameId);
        }

        async Task<List<CollectionModelWithLatestVersion>> DownloadCollections(
            IEnumerable<Tuple<Guid, List<Guid>>> gamesWithCollections) {
            var apiHost =
                //#if DEBUG
                //"https://auth.local.withsix.net";
                //#else
                "https://auth.withsix.com";
            //#endif
            var list = new List<CollectionModelWithLatestVersion>();
            foreach (var g in gamesWithCollections) {
                list.AddRange(await
                    Tools.Transfer.GetJson<List<CollectionModelWithLatestVersion>>(
                        new Uri(apiHost + "/api/collections?gameId=" + g.Item1 +
                                string.Join("", g.Item2.Select(x => "&ids=" + x))), GetToken())
                        .ConfigureAwait(false));
            }
            return list;
        }

        string GetToken() {
            var sContext = _locator.GetSettingsContext();
            return sContext.Settings.Secure.Login?.Authentication.AccessToken;
        }
    }

    public class CustomRepo
    {
        readonly Uri _uri;

        public CustomRepo(Uri uri) {
            _uri = uri;
        }

        protected virtual Dictionary<string, SixRepoModDto> Mods { get; set; } = new Dictionary<string, SixRepoModDto>()
            ;
        protected virtual ICollection<Uri> Hosts { get; } = new List<Uri>();

        public static Uri GetRepoUri(Uri r) {
            var url = r.ToString();
            return !url.EndsWith("config.yml")
                ? new Uri(url.Substring(0, url.Length - Path.GetFileName(r.AbsolutePath).Length) + "config.yml")
                : r;
        }

        public async Task Load(IStringDownloader downloader) {
            var config = await Tools.Transfer.GetYaml<SixRepoConfigDto>(_uri).ConfigureAwait(false);
            Mods = config.Mods;
            Hosts.Replace(config.Hosts);
        }

        // TODO: localOnly if no update available? - so for local diagnose etc..
        public async Task GetMod(string name, IAbsoluteDirectoryPath destination, IAbsoluteDirectoryPath packPath,
            StatusRepo status, bool force = false) {
            var mod = GetMod(name);
            var folder = destination.GetChildDirectoryWithName(mod.Key);

            var opts = GetOpts(packPath, status, mod);
            if (!folder.Exists) {
                await
                    Repository.Factory.Clone((Uri[]) opts["hosts"], folder.ToString(), opts)
                        .ConfigureAwait(false);
                return;
            }

            var rsyncDir = folder.GetChildDirectoryWithName(Repository.RepoFolderName);
            if (!force && rsyncDir.Exists && IsRightVersion(rsyncDir, mod))
                return;

            var repo = GetRepo(rsyncDir, folder, opts);
            await repo.Update(opts).ConfigureAwait(false);
        }

        public KeyValuePair<string, SixRepoModDto> GetMod(string name) {
            return Mods.First(x => x.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        bool IsRightVersion(IAbsoluteDirectoryPath rsyncDir, KeyValuePair<string, SixRepoModDto> mod) {
            var versionFile = rsyncDir.GetChildFileWithName(Repository.VersionFileName);
            if (!versionFile.Exists)
                return false;

            var repoInfo = TryReadRepoFile(versionFile);
            return repoInfo != null && repoInfo.Guid == mod.Value.Guid && repoInfo.Version == mod.Value.Version;
        }

        static Repository GetRepo(IAbsoluteDirectoryPath rsyncDir,
            IAbsoluteDirectoryPath folder, Dictionary<string, object> opts) {
            var repo = rsyncDir.Exists
                ? Repository.Factory.Open(folder.ToString(), opts)
                : Repository.Factory.Convert(folder.ToString(), opts);
            return repo;
        }

        Dictionary<string, object> GetOpts(IAbsoluteDirectoryPath packPath, StatusRepo status,
            KeyValuePair<string, SixRepoModDto> mod) {
            // pff, better use a real param object!
            return new Dictionary<string, object> {
                {"hosts", Hosts.Select(x => new Uri(x, mod.Key)).ToArray()},
                {"required_version", mod.Value.Version},
                {"required_guid", mod.Value.Guid},
                {"pack_path", packPath.ToString()},
                {"status", status}
            };
        }

        public bool HasMod(string name) {
            return Mods.Keys.ContainsIgnoreCase(name);
        }

        [Obsolete("Convert to new Yaml Deserializer")]
        RepoVersion TryReadRepoFile(IAbsoluteFilePath path) {
            try {
                return YamlExtensions.NewFromYamlFile<RepoVersion>(path);
            } catch (YamlParseException e) {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            } catch (YamlException e) {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            } catch (YamlExpectedOtherNodeTypeException e) {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            }
        }
    }
}