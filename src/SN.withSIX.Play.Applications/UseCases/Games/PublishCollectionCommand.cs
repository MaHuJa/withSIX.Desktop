// <copyright company="SIX Networks GmbH" file="PublishCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class PublishCollectionCommand : IAsyncRequest<UnitType>
    {
        public PublishCollectionCommand(Guid id, CollectionScope scope = CollectionScope.Unlisted,
            Guid? forkedCollectionId = null) {
            Id = id;
            Scope = scope;
            ForkedCollectionId = forkedCollectionId;
        }

        public Guid Id { get; }
        public CollectionScope Scope { get; }
        public Guid? ForkedCollectionId { get; }
    }

    public class PublishCollectionCommandHandler : IAsyncRequestHandler<PublishCollectionCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;
        readonly IUserSettingsStorage _storage;

        public PublishCollectionCommandHandler(IContentManager contentList, IConnectApiHandler api,
            IUserSettingsStorage storage) {
            _contentList = contentList;
            _api = api;
            _storage = storage;
        }

        public async Task<UnitType> HandleAsync(PublishCollectionCommand request) {
            var collection = _contentList.CustomCollections.First(x => x.Id == request.Id);

            try {
                await collection.Publish(_api, _contentList, request.Scope, request.ForkedCollectionId).ConfigureAwait(false);
            } catch (CollectionImageUploadException ex) {
                MainLog.Logger.WarnException("Image failure", ex);
            }

            await _storage.SaveNow().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}