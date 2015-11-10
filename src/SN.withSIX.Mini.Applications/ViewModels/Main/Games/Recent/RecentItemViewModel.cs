// <copyright company="SIX Networks GmbH" file="RecentItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using LaunchContent = SN.withSIX.Mini.Applications.Usecases.Main.Games.LaunchContent;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent
{
    public class RecentItemViewModel : ViewModel, IRecentItemViewModel
    {
        readonly ReactiveCommand<Unit> _abort;
        readonly ReactiveCommand<Unit> _action;
        readonly IReactiveCommand _switchFavorite;
        readonly ReactiveCommand<UnitType> _visit;
        bool _isFavorite;
        DateTime _lastUsed;
        ItemState _state;
        Func<object, Task> _executeAsync;
        readonly Func<object, Task> _defaultExecute;
        CancellationTokenSource _cts = null;

        public RecentItemViewModel(Guid id, Guid gameId, string name, Uri image, bool isVisitable,
            ISelectionCollectionHelper<PlayAction> actions) {
            Id = id;
            GameId = gameId;
            Name = name;
            Image = image;
            Actions = actions;

            var gameLockedObservable = this.GetGameLockObservable(gameId);
            _defaultExecute = async x => {
                using (_cts = new CancellationTokenSource()) {
                    await (Actions.SelectedItem == PlayAction.Play
                        ? RequestAsync(new Usecases.Main.Games.Recent.PlayContent(gameId, new ContentGuidSpec(Id), _cts.Token))
                        : RequestAsync(new LaunchContent(gameId, new ContentGuidSpec(Id),
                            action: Actions.SelectedItem.ToLaunchAction())))
                        .ConfigureAwait(false);
                }
            };
            _executeAsync = _defaultExecute;
            _action =
                ReactiveCommand.CreateAsyncTask(gameLockedObservable,
                    async x => await _executeAsync(x).ConfigureAwait(false))
                    .DefaultSetup("PlayRecentItem");

            _visit =
                ReactiveCommand.CreateAsyncTask(Observable.Return(isVisitable),
                    async x => await RequestAsync(new OpenContentLink(gameId, id)).ConfigureAwait(false))
                    .DefaultSetup("Visit");

            // TODO: Why doesnt the CanExecute observable work??
            var canExecute = _action.IsExecuting;
            _abort =
                ReactiveCommand.CreateAsyncTask(canExecute, async x => _cts.Cancel())
                    .DefaultSetup("AbortRecentItem");

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

            var observable = Listen<ContentUsed>()
                .Where(x => x.Content.Id == Id);
            observable.Where(x => x.Token != null)
                .Subscribe(x => UpdateExecute(x.Token));
            observable
                .Select(x => x.Content.RecentInfo.LastUsed)
                .ObserveOnMainThread()
                .BindTo(this, x => x.LastUsed);

            Listen<ContentFavorited>()
                .Where(x => x.Content.Id == Id)
                .Select(x => true)
                .ObserveOnMainThread()
                .BindTo(this, x => x.IsFavorite);

            Listen<ContentUnFavorited>()
                .Where(x => x.Content.Id == Id)
                .Select(x => false)
                .ObserveOnMainThread()
                .BindTo(this, x => x.IsFavorite);
        }

        internal void UpdateExecute(DoneCancellationTokenSource cts) {
            _cts = cts;
            _executeAsync = async o => {
                await Task.Run(async () => {
                    while (true) {
                        if (cts.IsCancellationRequested || cts.Disposed) {
                            _executeAsync = _defaultExecute;
                            return;
                        }
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            };
            RxApp.MainThreadScheduler.Schedule(() => Action.Execute(null));
        }

        public ISelectionCollectionHelper<PlayAction> Actions { get; }
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { this.RaiseAndSetIfChanged(ref _isFavorite, value); }
        }
        public ICommand SwitchFavorite => _switchFavorite;
        public ICommand Visit => _visit;
        public Uri Image { get; }
        public Guid Id { get; }
        public Guid GameId { get; set; }
        public string Name { get; }
        public DateTime LastUsed
        {
            get { return _lastUsed; }
            private set { this.RaiseAndSetIfChanged(ref _lastUsed, value); }
        }
        public IReactiveCommand Abort => _abort;
        public List<string> ContentNames { get; protected set; }
        public int ContentCount => ContentNames.Count;
        public IReactiveCommand Action => _action;

        public void RaisePlayedUpdated() {
            this.RaisePropertyChanged("LastPlayed");
        }

        public ItemState State
        {
            get { return _state; }
            private set { this.RaiseAndSetIfChanged(ref _state, value); }
        }
    }

    public interface IRecentItemViewModel : IHaveId<Guid>, IHaveItemState
    {
        IReactiveCommand Action { get; }
        string Name { get; }
        Uri Image { get; }
        Guid GameId { get; }
        DateTime LastUsed { get; }
        IReactiveCommand Abort { get; }
        int ContentCount { get; }
        List<string> ContentNames { get; }
        bool IsFavorite { get; }
        ICommand SwitchFavorite { get; }
        ICommand Visit { get; }
        ISelectionCollectionHelper<PlayAction> Actions { get; }
        void RaisePlayedUpdated();
    }
}