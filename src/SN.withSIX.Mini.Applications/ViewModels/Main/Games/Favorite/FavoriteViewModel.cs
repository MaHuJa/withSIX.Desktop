// <copyright company="SIX Networks GmbH" file="FavoriteViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Favorite
{
    public interface IFavoriteViewModel : IGameTabViewModel
    {
        IReactiveList<FavoriteItemViewModel> FavoriteItems { get; }
    }

    public class FavoriteViewModel : TabViewModel, IFavoriteViewModel
    {
        readonly Guid _id;

        public FavoriteViewModel(Guid id, IEnumerable<FavoriteItemViewModel> favoriteItems) {
            _id = id;
            FavoriteItems = new ReactiveList<FavoriteItemViewModel>(favoriteItems);

            Listen<ContentFavorited>()
                .Select(x => x.Content.MapTo<FavoriteItemViewModel>())
                .ObserveOnMainThread()
                .Subscribe(FavoriteItems.Add);

            Listen<ContentUnFavorited>()
                .ObserveOnMainThread()
                .Select(x => FavoriteItems.Find(x.Content.Id))
                .Subscribe(x => FavoriteItems.Remove(x));
        }

        public IReactiveList<FavoriteItemViewModel> FavoriteItems { get; }
        public override string DisplayName => "Favorite";
        public override string Icon => SixIconFont.withSIX_icon_Star_Outline;
    }
}