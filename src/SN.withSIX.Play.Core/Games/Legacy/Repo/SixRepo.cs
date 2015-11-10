// <copyright company="SIX Networks GmbH" file="SixRepo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MoreLinq;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Validators;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Core.Games.Legacy.Repo
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Sync.Core.Models.Repositories.SixSync"
        )]
    public class SixRepo : SelectionList<IContent>
    {
        public const string PwsProtocolRegex = "pws(https?|ftp|rsync|zsync)?://";
        public static readonly string[] URLSchemes = {"pws", "pwshttp", "pwshttps", "pwsftp", "pwsrsync", "pwszsync"};

        public SixRepo() {
            Config = new SixRepoConfig();
            Servers = new Dictionary<string, SixRepoServer>();
            Collections = new CustomCollection[0];
        }

        public SixRepo(string location) : this() {
            Location = location;
            LoadConfig();
        }

        public SixRepo(Uri uri)
            : this() {
            Uri = uri;
        }

        public CustomCollection[] CollectionItems { get; private set; }
        public string Name
        {
            get { return Config.Name ?? GetUriString(); }
            set
            {
                Config.Name = value;
                OnPropertyChanged();
            }
        }
        [DataMember]
        public bool RememberWarnOnRepoAvailabilty { get; set; }
        [DataMember]
        public SixRepoConfig Config { get; private set; }
        [DataMember]
        public Dictionary<string, SixRepoServer> Servers { get; private set; }
        public string Destination { get; set; }
        public string Location { get; }
        [DataMember]
        public Uri Uri { get; private set; }
        public Dictionary<string, SixRepoApp> Apps
        {
            get { return Config.Apps; }
        }
        public IMod[] Mods { get; set; }
        public CustomCollection[] Collections { get; set; }

        public Uri GetInfoUri(string serverName) {
            if (serverName != null && Servers.ContainsKey(serverName) && Servers[serverName].Info != null)
                return Servers[serverName].Info.ToUri();
            return Config.Homepage == null ? null : Config.Homepage.ToUri();
        }

        public virtual Task DownloadMissions(IAbsoluteDirectoryPath destination, StatusRepo repo) {
            return DownloadChangedAndNewMissions(GetMissions(Config.Missions).Values, destination, repo);
        }

        public virtual Task DownloadMPMissions(IAbsoluteDirectoryPath destination, StatusRepo repo) {
            return DownloadChangedAndNewMissions(GetMissions(Config.MPMissions, MissionTypes.MpMission).Values,
                destination, repo,
                "mpmissions");
        }

        public string GetUrl(string key) {
            return GetUri(key).ToString();
        }

        public Uri GetUri(string key) {
            return Tools.Transfer.JoinUri(Uri, key + ".yml");
        }

        // TODO: Just call when needed..
        public void UpdateMods(IEnumerable<IMod> networkMods) {
            Mods = GetMods(networkMods);
        }

        void UpdateItems(ISupportModding game) {
            var repoMods = Mods
                .Where(x => x.GameMatch(game))
                .ToArray();
            repoMods.ForEach(x => x.Controller.UpdateState(game));
            repoMods.SyncCollection(Items);

            CollectionItems = Collections.Where(x => !x.IsHidden && x.GameMatch(game)).ToArray();
        }

        public virtual ValidationErrors[] ValidateServerConfig(SixRepoServer server) {
            var validationErrors = new List<ValidationErrors>();
            server.Apps
                .Where(x => !Config.Apps.ContainsKey(x))
                .ForEach(
                    x =>
                        validationErrors.Add(new ValidationErrors {
                            Message = x + " is missing from " + Repository.ConfigFileName
                        }));

            return validationErrors.ToArray();
        }

        public virtual async Task LoadConfigRemote(IStringDownloader downloader) {
            var uri = Tools.Transfer.JoinUri(Uri, Repository.ConfigFileName);
            var data = await downloader.DownloadAsync(uri).ConfigureAwait(false);
            Config = YamlExtensions.NewFromYaml<SixRepoConfig>(data);
            if (Config.Hosts.Length == 0)
                Config.Hosts = new[] {Uri};

            Servers = (await Config.Servers.SelectAsync(y => GetServer(y, downloader))).ToDictionary(x => x.Key,
                x => x.Value);
        }

        async Task<KeyValuePair<string, SixRepoServer>> GetServer(string y, IStringDownloader downloader) {
            var uri = GetUri(y);
            try {
                return new KeyValuePair<string, SixRepoServer>(y,
                    YamlExtensions.NewFromYaml<SixRepoServer>(await downloader.DownloadAsync(uri).ConfigureAwait(false)));
            } catch (WebException e) {
                throw new TransferError(
                    String.Format("Problem while trying to access: {0}\n{1}",
                        uri.AuthlessUri(), e.Message), e);
            }
        }

        public void Reset(IContentManager contentList, ISupportModding game) {
            UpdateModSets(contentList);
            UpdateMods(contentList.Mods);
            UpdateItems(game);
        }

        void UpdateModSets(IContentManager contentList) {
            var modSets = Collections.Where(x => Servers.Keys.Contains(x.ServerKey)).ToArray();
            var serversToAdd = Servers
                .Where(x => !modSets.Select(y => y.ServerKey).Contains(x.Key))
                .Select(s => contentList.CreateCustomRepoServerModSet(s.Value, s.Key, this));
            Collections = modSets.Concat(serversToAdd).ToArray();
        }

        IMod[] GetMods(IEnumerable<IMod> networkMods) {
            var network = GetNetwork();
            return Config.Mods
                // TODO: Validate names on import and dont even save them
                .Where(x => FileNameValidator.IsValidName(x.Key))
                .Select(x => x.Value.ToMod(x.Key, network, networkMods))
                .ToArray();
        }

        string GetUriString() {
            return Uri != null ? Uri.Host : null;
        }

        // TODO: Still in place for Repo cache, probably should convert to use a DTO?
        [OnDeserialized]
        protected void OnDeserializedSr(StreamingContext sc) {
            if (Config == null)
                Config = new SixRepoConfig();

            if (Servers == null)
                Servers = new Dictionary<string, SixRepoServer>();

            if (Collections == null)
                Collections = new CustomCollection[0];
        }

        async Task DownloadChangedAndNewMissions(IEnumerable<Mission> missions, IAbsoluteDirectoryPath destination,
            StatusRepo repo,
            string type = "missions") {
            var location = Path.Combine(destination.ToString(), type).ToAbsoluteDirectoryPath();
            var downloadDictionary = await GetMissionsToDownload(missions, location, repo).ConfigureAwait(false);
            if (downloadDictionary.Any()) {
                await SyncEvilGlobal.DownloadHelper.DownloadFilesAsync(
                    Config.Hosts.Select(x => Tools.Transfer.JoinUri(x, type)).ToArray(), repo,
                    downloadDictionary,
                    location, 15).ConfigureAwait(false);
            }
        }

        static Task<IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus>>
            GetMissionsToDownload(
            IEnumerable<Mission> missions,
            IAbsoluteDirectoryPath destination, StatusRepo repo) {
            return Task.Factory.StartNew(() => SumMissions(destination, missions.ToDictionary(x => x,
                x => (ITransferStatus) new Status(x.FileName, repo))), TaskCreationOptions.LongRunning);
        }

        static IDictionary<KeyValuePair<string, Func<IAbsoluteFilePath, bool>>, ITransferStatus> SumMissions(
            IAbsoluteDirectoryPath destination,
            Dictionary<Mission, ITransferStatus> missionsDictionary) {
            return missionsDictionary.Where(m => {
                m.Value.Action = RepoStatus.Summing;
                var filePath = Path.Combine(destination.ToString(), m.Key.FileName).ToAbsoluteFilePath();
                return !filePath.Exists || Tools.HashEncryption.MD5FileHash(filePath) != m.Key.Md5;
            })
                .ToDictionary(x => new KeyValuePair<string, Func<IAbsoluteFilePath, bool>>(x.Key.FileName, null),
                    x => x.Value);
        }

        Dictionary<string, Mission> GetMissions(Dictionary<string, string> missions,
            string missionType = MissionTypes.SpMission) {
            var network = GetNetwork();
            return missions
                .ToDictionary(x => x.Key, x => CreateMission(x, missionType, network));
        }

        Network GetNetwork() {
            var network = new Network(Guid.Empty);
            network.Mirrors = Config.Hosts.Select(x => new Mirror(Guid.Empty) {Url = x, Network = network}).ToList();
            network.MaxThreads = Config.MaxThreads;
            return network;
        }

        static Mission CreateMission(KeyValuePair<string, string> x, string type, Network network) {
            return new Mission(Guid.NewGuid()) {
                Name = Mission.ValidMissionName(x.Key),
                FullName = Mission.NiceMissionName(x.Key),
                FileName = x.Key,
                Md5 = x.Value,
                Type = type,
                Networks = new[] {network}.ToList()
            };
        }

        void LoadConfig() {
            Config =
                YamlExtensions.NewFromYamlFile<SixRepoConfig>(
                    Path.Combine(Location, Repository.ConfigFileName).ToAbsoluteFilePath());
            OnPropertyChanged("Name");
            Servers =
                Config.Servers.Select(
                    y =>
                        new KeyValuePair<string, SixRepoServer>(y,
                            YamlExtensions.NewFromYamlFile<SixRepoServer>(
                                Path.Combine(Location, y + ".yml").ToAbsoluteFilePath())))
                    .ToDictionary(x => x.Key, x => x.Value);
        }

        public static bool IsServerUrl(string url) {
            return url.EndsWith(".yml") && !url.EndsWith(Repository.ConfigFileName);
        }

        public static Tuple<string, string> GetUrlInfo(string url) {
            var urlParts = url.Split('/');
            var fn = urlParts.Last();
            var idx = fn.LastIndexOf(".");
            var serverName = idx < 0 ? fn : fn.Substring(0, idx);

            return Tuple.Create(String.Join("/", urlParts.Take(urlParts.Length - 1)), serverName);
        }

        public bool GameMatch() {
            return CollectionItems.Any() || Items.Any();
        }
    }

    [DoNotObfuscate]
    public class TransferError : Exception
    {
        public TransferError(string format, WebException webException) : base(format, webException) {}
    }

    [DoNotObfuscate]
    public class ValidationErrors
    {
        public string Message { get; set; }
    }
}