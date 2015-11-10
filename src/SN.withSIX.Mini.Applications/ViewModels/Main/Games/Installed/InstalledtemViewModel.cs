// <copyright company="SIX Networks GmbH" file="InstalledtemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed
{
    public class InstalledItemViewModel : ViewModel, IInstalledItemViewModel
    {
        static readonly InstalledItemActions[] installedItemActionses = {
            InstalledItemActions.Play,
            InstalledItemActions.Uninstall
        };
        readonly IReactiveCommand _action;
        readonly Guid _gameId;
        readonly IReactiveCommand _switchFavorite;
        readonly ReactiveCommand<UnitType> _visit;
        bool _isEnabled;
        bool _isFavorite;
        ItemState _state;

        public InstalledItemViewModel(Guid id, Guid gameId, bool isFavorite, string name, string version, Uri image, bool isVisitable) {
            Id = id;
            _gameId = gameId;
            IsFavorite = isFavorite;
            Name = name;
            Image = image;
            Version = version;

            _visit =
                ReactiveCommand.CreateAsyncTask(Observable.Return(isVisitable),
                    async x => await RequestAsync(new OpenContentLink(gameId, id)).ConfigureAwait(false))
                    .DefaultSetup("Visit");

            var gameLockedObservable = this.GetGameLockObservable(gameId);
            _action = ReactiveCommand.CreateAsyncTask(gameLockedObservable,
                async x => {
                    var action = Actions.SelectedItem;
                    if (action == InstalledItemActions.Uninstall) {
                        var r =
                            await
                                Cheat.DialogManager.MessageBoxAsync(
                                    new MessageBoxDialogParams("Are you sure you wish to uninstall: " + Name,
                                        "Uninstall?", SixMessageBoxButton.OKCancel)).ConfigureAwait(false);
                        if (r != SixMessageBoxResult.OK)
                            return;
                    }
                    await RequestAsync(GetAction(action)).ConfigureAwait(false);
                })
                .DefaultSetup("PlayLocalItem");

            _switchFavorite =
                ReactiveCommand.CreateAsyncTask(gameLockedObservable,
                    async x =>
                        await (
                            IsFavorite
                                ? RequestAsync(new UnfavoriteContent(gameId, id)).ConfigureAwait(false)
                                : RequestAsync(new MakeContentFavorite(gameId, id)).ConfigureAwait(false)))
                    .DefaultSetup("SwitchFavorite");

            Actions.WhenAnyValue(x => x.SelectedItem)
                .Skip(1)
                .InvokeCommand(_action);

            Listen<ContentFavorited>()
                .Where(x => x.Content.Id == id)
                .Select(x => true)
                .ObserveOnMainThread()
                .BindTo(this, x => x.IsFavorite);
            Listen<ContentUnFavorited>()
                .Where(x => x.Content.Id == id)
                .Select(x => false)
                .ObserveOnMainThread()
                .BindTo(this, x => x.IsFavorite);
        }

        public string Version { get; }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            private set { this.RaiseAndSetIfChanged(ref _isFavorite, value); }
        }
        public ISelectionCollectionHelper<InstalledItemActions> Actions { get; } =
            installedItemActionses.ToSelectionCollectionHelper();
        public ICommand SwitchFavorite => _switchFavorite;
        public Guid Id { get; }
        public string Name { get; }
        public Uri Image { get; }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { this.RaiseAndSetIfChanged(ref _isEnabled, value); }
        }
        public ICommand Visit => _visit;
        public IReactiveCommand Action => _action;
        public ItemState State
        {
            get { return _state; }
            private set { this.RaiseAndSetIfChanged(ref _state, value); }
        }

        IAsyncRequest<UnitType> GetAction(InstalledItemActions selectedAction) {
            switch (selectedAction) {
            case InstalledItemActions.Play:
                return new PlayInstalledItem(_gameId, new ContentGuidSpec(Id));
            case InstalledItemActions.Uninstall:
                return new UninstallInstalledItem(_gameId, new ContentGuidSpec(Id));
            default: {
                throw new NotSupportedException(selectedAction + " is not supported");
            }
            }
        }
    }

    public interface IInstalledItemViewModel : IViewModel, IHaveId<Guid>, IHaveItemState
    {
        string Name { get; }
        Uri Image { get; }
        bool IsEnabled { get; set; }
        ICommand Visit { get; }
        IReactiveCommand Action { get; }
        ISelectionCollectionHelper<InstalledItemActions> Actions { get; }
        ICommand SwitchFavorite { get; }
        bool IsFavorite { get; }
        string Version { get; }
    }

    public enum InstalledItemActions
    {
        Play,
        Uninstall
    }
}