// <copyright company="SIX Networks GmbH" file="ShowCollectionVisibilityQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using ShortBus;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ShowCollectionVisibilityQuery : IRequest<CollectionVisibilityViewModel>
    {
        public ShowCollectionVisibilityQuery(Guid collectionId) {
            CollectionId = collectionId;
        }

        public Guid CollectionId { get; }
    }

    public class ShowCollectionVisibilityQueryHandler :
        IRequestHandler<ShowCollectionVisibilityQuery, CollectionVisibilityViewModel>
    {
        readonly IConnectApiHandler _apiHandler;
        readonly IContentManager _contentList;

        public ShowCollectionVisibilityQueryHandler(IContentManager contentList, IConnectApiHandler apiHandler) {
            _contentList = contentList;
            _apiHandler = apiHandler;
        }

        public CollectionVisibilityViewModel Handle(ShowCollectionVisibilityQuery request) {
            _apiHandler.ConfirmLoggedIn();

            var collection = _contentList.CustomCollections.First(x => x.Id == request.CollectionId);
            if (string.IsNullOrWhiteSpace(collection.Name))
                throw new CollectionNameMissingException();

            if (!collection.Items.Any())
                throw new CollectionEmptyException();

            return new CollectionVisibilityViewModel();
        }
    }
}