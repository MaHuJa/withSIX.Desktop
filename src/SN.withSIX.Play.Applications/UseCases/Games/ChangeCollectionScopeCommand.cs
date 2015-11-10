// <copyright company="SIX Networks GmbH" file="ChangeCollectionScopeCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class ChangeCollectionScopeCommand : IAsyncRequest<UnitType>
    {
        public ChangeCollectionScopeCommand(Guid id, CollectionScope scope) {
            Id = id;
            Scope = scope;
        }

        public Guid Id { get; }
        public CollectionScope Scope { get; }
    }

    public class ChangeCollectionScopeCommandHandler : IAsyncRequestHandler<ChangeCollectionScopeCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;
        readonly IUserSettingsStorage _storage;

        public ChangeCollectionScopeCommandHandler(IContentManager contentList, IConnectApiHandler api,
            IUserSettingsStorage storage) {
            _contentList = contentList;
            _api = api;
            _storage = storage;
        }

        public async Task<UnitType> HandleAsync(ChangeCollectionScopeCommand request) {
            var collection = _contentList.CustomCollections.First(x => x.Id == request.Id);

            await collection.ChangeScope(_api, request.Scope).ConfigureAwait(false);

            await _storage.SaveNow().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}