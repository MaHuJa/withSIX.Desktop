// <copyright company="SIX Networks GmbH" file="FavoriteItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using LaunchContent = SN.withSIX.Mini.Applications.Usecases.Main.Games.LaunchContent;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Favorite
{
    public interface IFavoriteItemViewModel : IViewModel, IHaveId<Guid>, IHaveGameId, IHaveItemState
    {
        string Name { get; }
        Uri Image { get; }
        ICommand Unfavorite { get; }
        IReactiveCommand Action { get; }
        ICommand Visit { get; }
        ISelectionCollectionHelper<PlayAction> Actions { get; }
    }

    public class FavoriteItemViewModel : ViewModel, IFavoriteItemViewModel
    {
        readonly ReactiveCommand<UnitType> _action;
        readonly bool _isLocal;
        readonly IReactiveCommand _unfavorite;
        readonly ReactiveCommand<UnitType> _visit;
        ItemState _state;

        public FavoriteItemViewModel(Guid id, Guid gameId, string name, Uri image, bool isVisitable, bool isLocal,
            ISelectionCollectionHelper<PlayAction> actions) {
            _isLocal = isLocal;
            Actions = actions;
            Id = id;
            GameId = gameId;
            Name = name;
            Image = image;

            _visit =
                ReactiveCommand.CreateAsyncTask(Observable.Return(isVisitable),
                    async x => await RequestAsync(new OpenContentLink(gameId, id)).ConfigureAwait(false))
                    .DefaultSetup("Visit");

            var gameLockedObservable = this.GetGameLockObservable(gameId);
            _action = ReactiveCommand.CreateAsyncTask(gameLockedObservable,
                async x => await PlayContent(gameId).ConfigureAwait(false))
                .DefaultSetup("Play");

            _unfavorite =
                ReactiveCommand.CreateAsyncTask(gameLockedObservable,
                    async x =>
                        await RequestAsync(new UnfavoriteContent(gameId, id)).ConfigureAwait(false))
                    .DefaultSetup("SwitchFavorite");

            Actions.WhenAnyValue(x => x.SelectedItem)
                .Skip(1)
                .InvokeCommand(_action);
        }

        public ISelectionCollectionHelper<PlayAction> Actions { get; }
        public string Name { get; }
        public Uri Image { get; }
        public ICommand Unfavorite => _unfavorite;
        public ICommand Visit => _visit;
        public IReactiveCommand Action => _action;
        public Guid Id { get; }
        public Guid GameId { get; }
        public ItemState State
        {
            get { return _state; }
            private set { this.RaiseAndSetIfChanged(ref _state, value); }
        }

        Task<UnitType> PlayContent(Guid gameId) {
            var contentSpec = new ContentGuidSpec(Id);
            if (Actions.SelectedItem != PlayAction.Play) {
                return
                    RequestAsync(new LaunchContent(gameId, contentSpec, action: Actions.SelectedItem.ToLaunchAction()));
            }
            return _isLocal
                ? RequestAsync(new PlayInstalledItem(gameId, contentSpec))
                : RequestAsync(new PlayContent(gameId, contentSpec));
        }
    }
}