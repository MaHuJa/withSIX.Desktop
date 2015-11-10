// <copyright company="SIX Networks GmbH" file="ContentApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Infra.Api.ContentApi.Dto;

namespace SN.withSIX.Play.Infra.Api.ContentApi
{
    public static class RepoHelper
    {
        public static string GetFullApiPath(string apiPath) {
            return String.Join("/", "api", "v" + CommonUrls.ContentApiVersion, apiPath);
        }
    }

    class ContentApiRepository<T, T2> : IContentApiRepository
        where T : class, ISyncBaseDto
        where T2 : class, ISyncBase
    {
        const string JsonExt = ".json";
        readonly string _apiPath;
        readonly IApiLocalObjectCacheManager _cacheManager;
        readonly string _fullApiPath;
        readonly MappingEngine _mappingEngine;
        readonly ContentRestApi _rest;

        public ContentApiRepository(string type, ContentRestApi rest, MappingEngine mappingEngine,
            IApiLocalObjectCacheManager cacheManager) {
            var multi = type.Pluralize();
            _apiPath = multi + JsonExt;
            _fullApiPath = RepoHelper.GetFullApiPath(_apiPath);
            _rest = rest;
            _mappingEngine = mappingEngine;
            _cacheManager = cacheManager;
            Items = new Dictionary<Guid, T2>();
        }

        public ContentApiRepository(ContentRestApi rest, MappingEngine mappingEngine,
            IApiLocalObjectCacheManager cacheManager) :
                this(typeof (T2).Name.ToUnderscore(), rest, mappingEngine, cacheManager) {}

        public Dictionary<Guid, T2> Items { get; private set; }

        public async Task<bool> TryLoadFromDisk() {
            var data = await LoadAndMapFromDisk().ConfigureAwait(false);
            if (data == null)
                return false;
            Items = MakeDictionary(data);
            return true;
        }

        public async Task LoadFromApi(string hash) {
            var data = await LoadAndMapFromApi(hash).ConfigureAwait(false);
            if (data != null)
                Items = MakeDictionary(data);
        }

        [Obsolete("nuts")]
        public IEnumerable GetValues() {
            return Items.Values;
        }

        public string Hash { get; private set; }

        static string GetShortHash(string data) {
            return GetShortHash(Encoding.UTF8.GetBytes(data));
        }

        static string GetShortHash(byte[] content) {
            return Convert.ToBase64String(content.Sha1()).TrimEnd('=');
        }

        async Task<IReadOnlyCollection<T2>> LoadAndMapFromDisk() {
            return Map(await LoadFromDisk().ConfigureAwait(false));
        }

        async Task<List<T>> LoadFromDisk() {
            // TODO: Without ExHandling?
            // TODO: Don't save the JSON representation but our own? But then if we have a bug in the client we can have wrong data in the cache so..
            // the other way around is that we can have bad json data in the cache...
            try {
                var data = await _cacheManager.GetObject<string>(_fullApiPath);
                Hash = GetShortHash(data);
                return JsonConvert.DeserializeObject<List<T>>(data, ContentRestApi.JsonSettings);
            } catch (KeyNotFoundException) {
                return null;
            }
        }

        async Task<List<T>> LoadFromApiAndSaveToDisk(string hash) {
            var data = await _rest.GetDataAsync<List<T>>(_apiPath + ".gz?v=" + hash).ConfigureAwait(false);
            await SaveDataToDisk(data.Item2).ConfigureAwait(false);
            Hash = GetShortHash(data.Item2);
            return data.Item1;
        }

        public T2 Get(Guid id) {
            lock (Items)
                return Items.ContainsKey(id) ? Items[id] : null;
        }

        public T2 GetOrCreate(Guid id) {
            return Get(id) ?? (T2) Activator.CreateInstance(typeof (T2), id);
        }

        async Task SaveDataToDisk(string data) {
            await _cacheManager.SetObject(_fullApiPath, data);
        }

        async Task<List<T2>> LoadAndMapFromApi(string hash) {
            return Map(await LoadFromApiAndSaveToDisk(hash).ConfigureAwait(false));
        }

        List<T2> Map(List<T> list) {
            return _mappingEngine.Map<List<T2>>(list);
        }

        static Dictionary<Guid, T2> MakeDictionary(IEnumerable<T2> data) {
            return data.ToDictionary(x => x.Id, y => y);
        }
    }
}