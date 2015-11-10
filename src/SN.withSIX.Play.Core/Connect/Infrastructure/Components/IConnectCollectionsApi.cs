// <copyright company="SIX Networks GmbH" file="IConnectCollectionsApi.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Api.Models.Collections;

namespace SN.withSIX.Play.Core.Connect.Infrastructure.Components
{
    public interface IConnectCollectionsApi
    {
        Task<CollectionModel> GetCollection(Guid collectionId);
        Task<CollectionVersionModel> GetCollectionVersion(Guid versionId);
        Task<CollectionPublishInfo> PublishCollection(CreateCollectionModel inputModel);
        Task<Guid> PublishNewCollectionVersion(AddCollectionVersionModel model);
        Task<List<CollectionModel>> GetSubscribedCollections();
        Task<List<CollectionModel>> GetOwnedCollections();
        Task UnsubscribeCollection(Guid collectionID);
        Task DeleteCollection(Guid collectionId);
        Task ChangeCollectionScope(Guid collectionId, CollectionScope scope);
        Task ChangeCollectionName(Guid collectionId, string name);
        Task<string> GenerateNewCollectionImage(Guid id);
        Task<string> UploadCollectionAvatar(IAbsoluteFilePath imagePath, Guid collectionId);
    }
}