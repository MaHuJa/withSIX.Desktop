// <copyright company="SIX Networks GmbH" file="AddContentToCollectionsCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.UseCases
{
    class AddContentToCollectionsCommand : IAsyncRequest<UnitType>
    {
        public AddContentToCollectionsCommand(Guid contentId, IReadOnlyCollection<Guid> collectionIds) {
            ContentId = contentId;
            CollectionIds = collectionIds;
        }

        public Guid ContentId { get; }
        public IReadOnlyCollection<Guid> CollectionIds { get; }
    }

    class AddContentToCollectionsCommandHandler : IAsyncRequestHandler<AddContentToCollectionsCommand, UnitType>
    {
        readonly IContentManager _contentList;

        public AddContentToCollectionsCommandHandler(IContentManager contentList) {
            _contentList = contentList;
        }

        public async Task<UnitType> HandleAsync(AddContentToCollectionsCommand request) {
            var mod = _contentList.Mods.First(x => x.Id == request.ContentId);
            foreach (var c in request.CollectionIds.Select(
                x =>
                    _contentList.CustomCollections.Cast<Collection>().First(i => i.Id == x)))
                c.AddModAndUpdateState(mod, _contentList);

            return UnitType.Default;
        }
    }
}