// <copyright company="SIX Networks GmbH" file="UnpublishCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class UnpublishCollectionCommand : IAsyncRequest<UnitType>
    {
        public UnpublishCollectionCommand(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class UnpublishCollectionCommandHandler : IAsyncRequestHandler<UnpublishCollectionCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;
        readonly UserSettings _settings;
        readonly IUserSettingsStorage _storage;

        public UnpublishCollectionCommandHandler(IContentManager contentList, IConnectApiHandler api,
            UserSettings settings, IUserSettingsStorage storage) {
            _contentList = contentList;
            _api = api;
            _settings = settings;
            _storage = storage;
        }

        public async Task<UnitType> HandleAsync(UnpublishCollectionCommand request) {
            var collection = _contentList.CustomCollections.First(x => x.Id == request.Id);

            await collection.DeleteOnline(_api, _contentList).ConfigureAwait(false);

            await _storage.SaveNow().ConfigureAwait(false);
            return UnitType.Default;
        }
    }
}