using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;
using YamlDotNet.Core;

namespace SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo
{
    public class CustomRepo
    {
        readonly Uri _uri;

        public CustomRepo(Uri uri)
        {
            _uri = uri;
        }

        protected virtual Dictionary<string, SixRepoModDto> Mods { get; set; } = new Dictionary<string, SixRepoModDto>()
            ;
        protected virtual ICollection<Uri> Hosts { get; } = new List<Uri>();

        public static Uri GetRepoUri(Uri r)
        {
            var url = r.ToString();
            return !url.EndsWith("config.yml")
                ? new Uri(url.Substring(0, url.Length - Path.GetFileName(r.AbsolutePath).Length) + "config.yml")
                : r;
        }

        public async Task Load(IStringDownloader downloader)
        {
            var config = await Tools.Transfer.GetYaml<SixRepoConfigDto>(_uri).ConfigureAwait(false);
            Mods = config.Mods;
            Hosts.Replace(config.Hosts);
        }

        // TODO: localOnly if no update available? - so for local diagnose etc..
        public async Task GetMod(string name, IAbsoluteDirectoryPath destination, IAbsoluteDirectoryPath packPath,
            StatusRepo status, bool force = false)
        {
            var mod = GetMod(name);
            var folder = destination.GetChildDirectoryWithName(mod.Key);

            var opts = GetOpts(packPath, status, mod);
            if (!folder.Exists)
            {
                await
                    Repository.Factory.Clone((Uri[])opts["hosts"], folder.ToString(), opts)
                        .ConfigureAwait(false);
                return;
            }

            var rsyncDir = folder.GetChildDirectoryWithName(Repository.RepoFolderName);
            if (!force && rsyncDir.Exists && IsRightVersion(rsyncDir, mod))
                return;

            var repo = GetRepo(rsyncDir, folder, opts);
            await repo.Update(opts).ConfigureAwait(false);
        }

        public KeyValuePair<string, SixRepoModDto> GetMod(string name)
        {
            return Mods.First(x => x.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        bool IsRightVersion(IAbsoluteDirectoryPath rsyncDir, KeyValuePair<string, SixRepoModDto> mod)
        {
            var versionFile = rsyncDir.GetChildFileWithName(Repository.VersionFileName);
            if (!versionFile.Exists)
                return false;

            var repoInfo = TryReadRepoFile(versionFile);
            return repoInfo != null && repoInfo.Guid == mod.Value.Guid && repoInfo.Version == mod.Value.Version;
        }

        static Repository GetRepo(IAbsoluteDirectoryPath rsyncDir,
            IAbsoluteDirectoryPath folder, Dictionary<string, object> opts)
        {
            var repo = rsyncDir.Exists
                ? Repository.Factory.Open(folder.ToString(), opts)
                : Repository.Factory.Convert(folder.ToString(), opts);
            return repo;
        }

        Dictionary<string, object> GetOpts(IAbsoluteDirectoryPath packPath, StatusRepo status,
            KeyValuePair<string, SixRepoModDto> mod)
        {
            // pff, better use a real param object!
            return new Dictionary<string, object> {
                {"hosts", Hosts.Select(x => new Uri(x, mod.Key)).ToArray()},
                {"required_version", mod.Value.Version},
                {"required_guid", mod.Value.Guid},
                {"pack_path", packPath.ToString()},
                {"status", status}
            };
        }

        public bool HasMod(string name)
        {
            return Mods.Keys.ContainsIgnoreCase(name);
        }

        [Obsolete("Convert to new Yaml Deserializer")]
        RepoVersion TryReadRepoFile(IAbsoluteFilePath path)
        {
            try
            {
                return YamlExtensions.NewFromYamlFile<RepoVersion>(path);
            }
            catch (YamlParseException e)
            {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            }
            catch (YamlException e)
            {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            }
            catch (YamlExpectedOtherNodeTypeException e)
            {
                //this.Logger().FormattedWarnException(e, _mod.Name);
                return new RepoVersion();
            }
        }
    }
}
