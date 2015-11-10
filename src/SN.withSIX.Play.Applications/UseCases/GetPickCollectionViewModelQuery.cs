// <copyright company="SIX Networks GmbH" file="GetPickCollectionViewModelQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases
{
    class GetPickCollectionViewModelQuery : IAsyncRequest<PickCollectionViewModel>
    {
        public GetPickCollectionViewModelQuery(Guid modId, Guid gameId) {
            ModId = modId;
            GameId = gameId;
        }

        public Guid ModId { get; }
        public Guid GameId { get; }
    }

    class GetPickCollectionViewModelQueryHandler :
        IAsyncRequestHandler<GetPickCollectionViewModelQuery, PickCollectionViewModel>
    {
        readonly IContentManager _contentList;
        readonly Func<PickCollectionViewModel> _factory;

        public GetPickCollectionViewModelQueryHandler(Func<PickCollectionViewModel> factory,
            IContentManager contentList) {
            _factory = factory;
            _contentList = contentList;
        }

        public async Task<PickCollectionViewModel> HandleAsync(GetPickCollectionViewModelQuery request) {
            var vm = _factory();
            vm.SetContent(_contentList.Mods.FirstOrDefault(x => x.Id == request.ModId));
            vm.LoadItems(
                _contentList.CustomCollections.Where(x => x.GameId == request.GameId)
                    .Select(x => new PickCollectionDataModel {Id = x.Id, Name = x.Name, Image = x.Image ?? x.ImageLarge}));
            return vm;
        }
    }

    public class PickCollectionDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}