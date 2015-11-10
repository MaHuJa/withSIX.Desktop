// <copyright company="SIX Networks GmbH" file="ICollectionsHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Api.Models.Shared;

namespace SN.withSIX.Play.Infra.Api.Hubs
{
    [DoNotObfuscateType]
    interface ICollectionsHub : IClientHubProxyBase
    {
        Task<List<CollectionModel>> GetSubscribedCollections();
        Task<List<CollectionModel>> GetOwnedCollections();
        Task<CollectionModel> GetCollection(Guid collectionID);
        Task<CollectionVersionModel> GetCollectionVersion(Guid versionGuid);
        Task Subscribe(Guid collectionID);
        Task Unsubscribe(Guid collectionID);
        Task ChangeScope(Guid collectionID, CollectionScope scope);
        Task Delete(Guid collectionID);
        Task UpdateCollectionName(Guid collectionID, string name);
        Task<Guid> AddCollectionVersion(CreateCollectionVersionModel collectionVersion);
        Task<Guid> CreateNewCollection(CreateCollectionModel collection);

        [Obsolete("Please use RequestAvatarUpload instead")]
        Task UploadAvatar(Guid id, Guid collectionId, string part, Tuple<int, int> partNumber);

        Task<AWSUploadPolicy> RequestAvatarUpload(string fileName, Guid collectionId);
        Task<string> AvatarUploadCompleted(Guid collectionId, string uploadKey);
        Task<string> GenerateNewAvatarImage(Guid collectionId);
        IDisposable Subscribed(Action<SubscribedToCollection> action);
        IDisposable Unsubscribed(Action<UnsubscribedFromCollection> action);
        IDisposable Updated(Action<CollectionUpdated> action);
        IDisposable VersionAdded(Action<CollectionVersionAdded> action);
    }
}