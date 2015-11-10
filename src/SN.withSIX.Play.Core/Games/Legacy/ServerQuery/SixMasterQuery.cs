// <copyright company="SIX Networks GmbH" file="SixMasterQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    /*public class SixMasterQuery : IMasterServerQuery, IEnableLogging
    {
        const string StrServerSyncFailed =
            "Unable to retrieve the server list. Possible connection issues.\nWill try to load local cached server list...\n\nThe most common causes for this problem:\n- Security suite interference (Firewall/AntiVirus/etc)\n- Temporary internet connection issues\n- Disk space or permission issues\n- Files/Directories in use because they are open in Windows Explorer or other programs\n- Temporary SIX API unreachable (try again later)\n- Software bug\n\nSee the troubleshooting guide for more details, solutions and workarounds:\nhttps://community.withsix.com";

        readonly IDataDownloader _downloader;
        readonly string _serverBrowserTag;

        public SixMasterQuery(string serverBrowserTag, IDataDownloader downloader) {
            _serverBrowserTag = serverBrowserTag;
            _downloader = downloader;
        }

        public async Task<IEnumerable<ServerQueryResult>> GetParsedServers(bool forceLocal = false,
            int limit = 0) {
            return
                await
                    Task.Factory.StartNew(
                        () => DoStuff(forceLocal, limit).Select(x => new GamespyServerQueryResult(x, true)),
                        TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        IEnumerable<IDictionary<string, string>> DoStuff(bool forceLocal, int limit) {
            var dl = GetData(forceLocal);
            return String.IsNullOrWhiteSpace(dl) ? null : ParseServers(limit, dl);
        }

        string GetData(bool forceLocal) {
            var cache = GetCacheFilePath();
            return !forceLocal ? FetchListAndCache(cache) : ReadCacheFile(cache);
        }

        static string ReadCacheFile(IAbsoluteFilePath cache) {
            return cache.Exists ? File.ReadAllText(cache.ToString()) : String.Empty;
        }

        IAbsoluteFilePath GetCacheFilePath() {
            var cachePath = Common.Paths.CachePath.GetChildDirectoryWithName("Serverlists");
            cachePath.MakeSurePathExists();
            return cachePath.GetChildFileWithName(String.Format("{0}.txt", _serverBrowserTag));
        }

        IEnumerable<IDictionary<string, string>> ParseServers(int limit, string dl) {
            var list = dl
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                .AsParallel()
                .Select(ParseLine)
                .Where(x => x != null)
                .DistinctBy(x => x["address"])
                .OrderByDescending(x => x["numplayers"].TryInt());

            return limit > 0 ? list.Take(limit) : list;
        }

        IDictionary<string, string> ParseLine(string line) {
            var indexOfFirstSpace = line.IndexOf(' ');
            if (indexOfFirstSpace < 0)
                return null;
            var address = ParseServerAddress(line, indexOfFirstSpace);
            return address == null
                ? null
                : CreateServerDictionary(line.Substring(indexOfFirstSpace + 1), new ServerAddress(address));
        }

        static string ParseServerAddress(string line, int indexOfFirstSpace) {
            var address = line.Substring(0, indexOfFirstSpace);
            return address.Split(':').Length < 2 ? null : address;
        }

        IDictionary<string, string> CreateServerDictionary(string data, ServerAddress address) {
            var infos = data.Split(new[] {@"\\\\"}, StringSplitOptions.None);
            var hostname = infos[0];
            var mod = String.Empty;
            var numPlayers = String.Empty;
            var maxPlayers = String.Empty;
            try {
                numPlayers = infos[1];
                maxPlayers = infos[2];
                mod = infos[3];
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }

            return new Dictionary<string, string> {
                {"address", address.ToString()},
                {"hostname", hostname},
                {"mod", mod},
                {"gamename", _serverBrowserTag},
                {"numplayers", numPlayers},
                {"maxplayers", maxPlayers}
            };
        }

        string FetchListAndCache(IAbsoluteFilePath cacheFile) {
            try {
                var dl = FetchServerList();
                if (string.IsNullOrWhiteSpace(dl)) {
                    this.Logger().Warn("server list appears empty " + _serverBrowserTag);
                    return ReadCacheFile(cacheFile);
                }
                TryWriteCachedList(cacheFile, dl);
                return dl;
            } catch (Exception e) {
                Tools.Diagnostic.NonFatalException(e, StrServerSyncFailed,
                    "A problem has occurred during server list fetch");
                return ReadCacheFile(cacheFile);
            }
        }

        string FetchServerList() {
            var dlB = _downloader.Download(GetServerlistUrl(_serverBrowserTag));
            return dlB.Any() ? Encoding.UTF8.GetString(Tools.Compression.Gzip.DecompressGzip(dlB)) : String.Empty;
        }

        void TryWriteCachedList(IAbsoluteFilePath cache, string dl) {
            try {
                Common.Paths.LocalDataPath.MakeSurePathExists();
                File.WriteAllText(cache.ToString(), dl);
            } catch (Exception e) {
                this.Logger().FormattedErrorException(e);
            }
        }

        static Uri GetServerlistUrl(string serverBrowserTag) {
            return Tools.Transfer.JoinUri(CommonUrls.ApiCdnUrl,
                String.Format("server_list_basic_{0}_v1.txt.gz", serverBrowserTag));
        }
    }*/
}