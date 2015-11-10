// <copyright company="SIX Networks GmbH" file="ConnectApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Chat;
using SN.withSIX.Api.Models.Social;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    abstract class ConnectApiRepository<T> where T : class, IHaveGuidId
    {
        readonly IDictionary<Guid, T> _store = new Dictionary<Guid, T>();
        protected readonly IConnectionManager ConnectionManager;
        protected readonly MappingEngine MappingEngine;

        protected ConnectApiRepository(IConnectionManager connectionManager, MappingEngine mappingEngine) {
            ConnectionManager = connectionManager;
            MappingEngine = mappingEngine;
        }

        public void Add(T obj) {
            lock (_store)
                _store.Add(obj.Id, obj);
        }

        void AddWhenMissing(T obj) {
            lock (_store) {
                if (!_store.ContainsKey(obj.Id))
                    _store.Add(obj.Id, obj);
            }
        }

        public virtual async Task RefreshAsync(T obj) {
            var dObj = await GetFromApiAsync(obj.Id).ConfigureAwait(false);
            lock (obj)
                MappingEngine.Map(dObj, obj);
        }

        public T Get(Guid id) {
            lock (_store)
                return _store.ContainsKey(id) ? _store[id] : null;
        }

        public async Task<T> GetOrRetrieveAndAddAsync(Guid uuid) {
            //lock (Store)
            return Get(uuid) ?? await GetFromApiAndAddAsync(uuid).ConfigureAwait(false);
        }

        public T GetOrCreateAndAdd(Guid uuid) {
            lock (_store)
                return Get(uuid) ?? CreateAndAdd(uuid);
        }

        public void Import<T2>(IEnumerable<T2> records) where T2 : UniqueApiModel {
            foreach (var item in records)
                UpdateOrAdd(item);
        }

        public void UpdateOrAdd<T2>(T2 item) where T2 : UniqueApiModel {
            lock (_store) {
                var existingRecord = Get(item.Id);
                if (existingRecord == null)
                    AddWhenMissing(MappingEngine.Map<T>(item));
                else
                    MappingEngine.Map(item, existingRecord);
            }
        }

        T CreateAndAdd(Guid uuid) {
            var obj = Create(uuid);
            Add(obj);
            return obj;
        }

        static T Create(Guid uuid) {
            return (T) Activator.CreateInstance(typeof (T), uuid);
        }

        async Task<T> GetFromApiAndAddAsync(Guid uuid) {
            var obj = await GetFromApiAsync(uuid).ConfigureAwait(false);
            AddWhenMissing(obj);
            return obj;
        }

        async Task<T> GetFromApiAsync(Guid uuid) {
            return MappingEngine.Map<T>(await GetDtoAsync(uuid).ConfigureAwait(false));
        }

        Task<object> GetDtoAsync(Guid uuid) {
            return Get<T>(uuid);
        }

        public Task<object> Get<T>(Guid id) {
            if (!ConnectionManager.IsConnected())
                throw new NotConnectedException();
            var t = typeof (T);
/*
            if (t == typeof (Account))
                return ConnectionManager.AccountHub.GetAccount(id).To<AccountModel, object>();
*/

            throw new UnsupportedTypeException();
        }
    }

    [DoNotObfuscate]
    class UnsupportedTypeException : Exception {}
}