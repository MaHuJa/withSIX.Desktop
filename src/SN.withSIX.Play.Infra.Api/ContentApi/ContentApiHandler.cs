// <copyright company="SIX Networks GmbH" file="ContentApiHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using Newtonsoft.Json;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services.Infrastructure;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Infra.Api.ContentApi.Dto;

namespace SN.withSIX.Play.Infra.Api.ContentApi
{
    static class ApiHashesExtension
    {
        // TODO: Horrible implementation, need to remain in sync with _repositoreis of Api!!
        public static List<string> ToList(this ApiHashes hashes) {
            return new List<string> {
                hashes.Categories,
                hashes.Mods,
                hashes.ModSets,
                hashes.Missions,
                // unused
                hashes.Families,
                hashes.Mirrors,
                hashes.Networks
            };
        }

        public static ApiHashes ToApiHashes(this List<string> hashes) {
            return new ApiHashes {
                Categories = hashes[0],
                Mods = hashes[1],
                ModSets = hashes[2],
                Missions = hashes[3]
                // xx
            };
        }
    }

    class ContentApiHandler : IContentApiHandler, IInfrastructureService
    {
        const string HashesJson = "hashes.json";
        readonly IApiLocalObjectCacheManager _cacheManager;
        readonly ContentApiRepository<CategoryDto, Category> _categoryRepository;
        readonly ContentApiRepository<MissionDto, Mission> _missionRepository;
        readonly ContentApiRepository<ModDto, Mod> _modRepository;
        readonly ContentApiRepository<ModSetDto, Collection> _modSetRepository;
        readonly IContentApiRepository[] _repositories;
        readonly ContentRestApi _rest;
        readonly UserSettings _settings;
        ApiHashes _localHashes;
        ApiHashes _remoteHashes;

        public ContentApiHandler(UserSettings settings, IApiLocalObjectCacheManager cacheManager) {
            _rest = new ContentRestApi();
            _settings = settings;
            _cacheManager = cacheManager;
            var mappingEngine = GetMapper();

            _remoteHashes = new ApiHashes();
            _localHashes = new ApiHashes();
            _modRepository = new ContentApiRepository<ModDto, Mod>(_rest, mappingEngine, cacheManager);
            _categoryRepository = new ContentApiRepository<CategoryDto, Category>(_rest, mappingEngine, cacheManager);
            _missionRepository = new ContentApiRepository<MissionDto, Mission>("mission", _rest, mappingEngine,
                cacheManager);

            /*
            _familyRepository = new ContentApiRepository<GameFamilyDto, GameFamily>("families", _rest, mappingEngine,
                cacheManager);
*/
            _modSetRepository = new ContentApiRepository<ModSetDto, Collection>("mod_set", _rest, mappingEngine,
                cacheManager);
            /*
            _mirrorRepository = new ContentApiRepository<MirrorDto, Mirror>(_rest, mappingEngine, cacheManager);
            _networkRepository = new ContentApiRepository<NetworkDto, Network>(_rest, mappingEngine, cacheManager);
*/
            // _familyRepository, _mirrorRepository, _networkRepository, 
            _repositories = new IContentApiRepository[]
            {_categoryRepository, _modRepository, _modSetRepository, _missionRepository};
        }

        public bool Loaded { get; private set; }

        public async Task LoadFromDisk() {
            var loaded = true;
            var newHashes = new List<string>();
            foreach (var repo in _repositories) {
                if (!(await repo.TryLoadFromDisk().ConfigureAwait(false)))
                    loaded = false;
                newHashes.Add(repo.Hash);
            }

            Loaded = loaded;

            // This might be overzealous, but at least fixes the current issue (otherwise had to restart twice to fix current issue)
            _localHashes = newHashes.ToApiHashes(); //await GetHashes().ConfigureAwait(false);
        }

        public List<T> GetList<T>() {
            return _repositories.First(x => x.GetType().GenericTypeArguments[1] == typeof (T))
                .GetValues().Cast<T>().ToList();
        }

        public async Task<bool> LoadFromApi() {
            var data = (await _rest.GetDataAsync<ApiHashes>(HashesJson + ".gz").ConfigureAwait(false));
            return await LoadFromApi(data.Item1).ConfigureAwait(false);
        }

        public async Task<bool> LoadFromApi(ApiHashes hashes) {
            _remoteHashes = hashes;

            var remoteList = _remoteHashes.ToList();
            var localList = _localHashes.ToList();
            var newHashes = new List<string>();

            var changed = false;

            //return Task.WhenAll(_repositories.Select(x => x.LoadFromApi()));
            // Temporary doing in foreach loop because we load and map at the same time currently, while we intend for items to rely on eachother..
            var i = 0;
            foreach (var repo in _repositories) {
                var j = i++;
                // TODO: Horrible index based, needs to keep ToList and _repositories into sync!! :<
                var remoteHash = remoteList[j];
                var localHash = localList[j];
                if (remoteHash != localHash) {
                    await repo.LoadFromApi(remoteHash).ConfigureAwait(false);
                    changed = true;
                }
                newHashes.Add(repo.Hash);
            }
            var newHash = newHashes.ToApiHashes();
            await _cacheManager.SetObject(RepoHelper.GetFullApiPath(HashesJson), newHash.ToJson());
            _localHashes = newHash;

            return changed;
        }

        async Task<ApiHashes> GetHashes() {
            // TODO: Without ExHandling?
            // TODO: Don't save the JSON representation but our own? But then if we have a bug in the client we can have wrong data in the cache so..

            try {
                var data = await _cacheManager.GetObject<string>(RepoHelper.GetFullApiPath(HashesJson));
                return JsonConvert.DeserializeObject<ApiHashes>(data, ContentRestApi.JsonSettings);
            } catch (KeyNotFoundException) {
                return new ApiHashes();
            }
        }

        MappingEngine GetMapper() {
            var mapConfig = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            mapConfig.SetupConverters();

            mapConfig.CreateMap<CategoryDto, Category>()
                .ConstructUsing(input => _categoryRepository.GetOrCreate(input.Id));

            mapConfig.CreateMap<MissionDto, Mission>()
                .ConstructUsing(input => _missionRepository.GetOrCreate(input.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.PackageName))
                .ForMember(x => x.Island, opt => opt.MapFrom(src => src.Map))
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.Name))
                .AfterMap(AfterMapping);

            mapConfig.CreateMap<ModDto, Mod>()
                .ConstructUsing(input => _modRepository.GetOrCreate(input.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.PackageName))
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.Name))
                /*
                .ForMember(x => x.Networks,
                    opt =>
                        opt.ResolveUsing(
                            src => src.Networks == null
                                ? new List<Network>()
                                : src.Networks.Select(
                                    n => _networkRepository.Get(n.Uuid))
                                    .Where(n => n != null)))
*/
                .ForMember(x => x.Categories, opt => opt.ResolveUsing(GetModCategoriesOrDefault))
                .ForMember(x => x.Aliases,
                    opt => opt.ResolveUsing(src => src.Aliases == null ? new string[0] : src.Aliases.Split(';')))
                .AfterMap(AfterMapping);

            mapConfig.CreateMap<ModSetDto, Collection>()
                .ConstructUsing(input => _modSetRepository.GetOrCreate(input.Id))
                .ForMember(x => x.GameId, opt => {
                    opt.MapFrom(x => x.GameUuid);
                    opt.NullSubstitute(Collection.DefaultGameUuid);
                })
                .AfterMap((src, dst) => dst.IsFavorite = _settings.ModOptions.IsFavorite(dst));

            /*            mapConfig.CreateMap<NetworkDto, Network>()
                .ConstructUsing(input => _networkRepository.GetOrCreate(input.Uuid))
                .ForMember(x => x.Mirrors,
                    opt =>
                        opt.ResolveUsing(
                            src =>
                                _mirrorRepository.Items.Select(x => x.Value)
                                    .Where(mirror => mirror.NetworkId == src.Uuid)))
                .AfterMap((src, dst) => dst.Mirrors.ForEach(mirror => mirror.Network = dst));

            mapConfig.CreateMap<MirrorDto, Mirror>()
                .ConstructUsing(input => _mirrorRepository.GetOrCreate(input.Uuid));

            mapConfig.CreateMap<GameFamilyDto, GameFamily>()
                .ConstructUsing(input => _familyRepository.GetOrCreate(input.Uuid));*/

            return new MappingEngine(mapConfig);
        }

        void AfterMapping(ModDto src, Mod dst) {
            dst.IsFavorite = _settings.ModOptions.IsFavorite(dst);
            if (src.ImagePath != null) {
                dst.Image =
                    Tools.Transfer.JoinUri(CommonUrls.UsercontentCdnProduction, src.ImagePath)
                        .ToString();
            }
        }

        void AfterMapping(MissionDto src, Mission dst) {
            dst.IsFavorite = _settings.MissionOptions.IsFavorite(dst);
            var list = new List<string>();
            dst.Type = MissionTypes.SpMission;
            if (src.Playability.HasFlag(Playability.Singleplayer))
                list.Add(MissionTypesHunan.SpMission);
            if (src.Playability.HasFlag(Playability.Multiplayer)) {
                dst.Type = MissionTypes.MpMission;
                list.Add(MissionTypesHunan.MpMission);
            }
            dst.Types = list.ToArray();
            if (src.ImagePath != null) {
                dst.Image =
                    Tools.Transfer.JoinUri(CommonUrls.UsercontentCdnProduction, src.ImagePath, "160x100.jpg")
                        .ToString();
            }
        }

        string[] GetModCategoriesOrDefault(ModDto src) {
            var categories = GetModCategories(src).ToArray();
            return categories.Any() ? categories.Select(x => x.ToString()).ToArray() : new[] {Common.DefaultCategory};
        }

        IEnumerable<Category> GetModCategories(ModDto src) {
            return
                src.Categories.Select(n => _categoryRepository.Get(n.Id))
                    .Where(n => n != null);
        }
    }
}