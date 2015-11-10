// <copyright company="SIX Networks GmbH" file="DeleteCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class DeleteCollectionCommand : IAsyncRequest<UnitType>
    {
        public DeleteCollectionCommand(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class DeleteCollectionCommandHandler : IAsyncRequestHandler<DeleteCollectionCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;
        readonly IUserSettingsStorage _storage;

        public DeleteCollectionCommandHandler(IContentManager contentList, IConnectApiHandler api,
            IUserSettingsStorage storage) {
            _contentList = contentList;
            _api = api;
            _storage = storage;
        }

        public async Task<UnitType> HandleAsync(DeleteCollectionCommand request) {
            var collection = _contentList.CustomCollections.First(x => x.Id == request.Id);

            if (collection.PublishedId.HasValue) {
                try {
                    await collection.DeleteOnline(_api, _contentList).ConfigureAwait(false);
                } catch (NotFoundException) {}
            }

            _contentList.CustomCollections.Remove(collection);

            await _storage.SaveNow().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}