// <copyright company="SIX Networks GmbH" file="StateHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.Services
{
    public class StatusModelChanged : IDomainEvent
    {
        public StatusModelChanged(StatusModel model) {
            Status = model;
        }

        public StatusModel Status { get; }
    }

    // TODO: Sort out ViewModel vs Service. Currently we use this object to centralize the information for both the
    // XAML ViewModels, as well as the JS ViewModels
    public interface IStateHandler
    {
        IObservable<StatusModel> StatusObservable { get; }
        IDictionary<Guid, GameStateHandler> Games { get; }
        StatusModel Status { get; }
        Task Initialize();
    }

    public class StateHandler : ViewModel, IApplicationService, IStateHandler
    {
        readonly IDbContextLocator _locator;
        readonly Subject<StatusModel> _subject;
        StatusModel _status = StatusModel.Default;

        public StateHandler(IDbContextLocator locator) {
            _locator = locator;
            Listen<StatusChanged>()
                .Subscribe(Handle);
            Listen<ContentStatusChanged>()
                .Subscribe(Handle);
            Listen<LocalContentAdded>()
                .Subscribe(Handle);
            Listen<UninstallActionCompleted>()
                .Subscribe(Handle);
            Listen<GameLaunched>()
                .Subscribe(Handle);
            Listen<GameTerminated>()
                .Subscribe(Handle);

            _subject = new Subject<StatusModel>();
            _subject.Subscribe(x => new StatusModelChanged(x).RaiseEvent().WaitAndUnwrapException());

            this.WhenAnyValue(x => x.Status)
                .Subscribe(x => _subject.OnNext(x));
        }

        public IObservable<StatusModel> StatusObservable => _subject.StartWith(Status);
        public IDictionary<Guid, GameStateHandler> Games { get; } = new Dictionary<Guid, GameStateHandler>();
        public StatusModel Status
        {
            get { return _status; }
            private set { this.RaiseAndSetIfChanged(ref _status, value); }
        }

        public async Task Initialize() {
            var context = _locator.GetReadOnlyGameContext();
            await context.LoadAll().ConfigureAwait(false);

            // TODO: Only states of games that are actually used in the session (so progressively?)
            foreach (var g in context.Games) {
                Games[g.Id] =
                    new GameStateHandler(
                        new ConcurrentDictionary<Guid, ContentState>(
                            g.LocalContent.GetStates()
                                .Concat(g.Collections.GetStates())
                                .ToDictionary(x => x.Key, x => x.Value)));
            }
        }

        void Handle(GameLaunched message) {
            Games[message.Game.Id].IsRunning = true;
            var t = Task.Run(async () => {
                try {
                    using (var process = Process.GetProcessById(message.ProcessId))
                        process.WaitForExit();
                } finally {
                    await new GameTerminated(message.Game, message.ProcessId).Raise().ConfigureAwait(false);
                }
            });
        }

        void Handle(GameTerminated message) {}

        void Handle(UninstallActionCompleted message) {
            var gameState = Games[message.Game.Id].State;
            foreach (var c in message.UninstallLocalContentAction.Content) {
                ContentState cs;
                gameState.TryRemove(c.Content.Id, out cs);
            }
        }

        void Handle(LocalContentAdded message) {
            var gameState = Games[message.GameId].State;
            foreach (var c in message.LocalContent)
                gameState[c.ContentId] = c.MapTo<ContentState>();
        }

        void Handle(ContentStatusChanged message) {
            var gameState = Games[message.Content.GameId].State;
            ContentState state;
            switch (message.State) {
            case ItemState.Uninstalled: {
                gameState.TryRemove(message.Content.Id, out state);
                return;
            }
            case ItemState.NotInstalled: {
                gameState.TryRemove(message.Content.Id, out state);
                return;
            }
            }
            gameState[message.Content.Id] = message.MapTo<ContentState>();
        }

        void Handle(StatusChanged message) {
            switch (message.Status) {
            case Mini.Core.Games.Services.ContentInstaller.Status.Synchronized: {
                Status = StatusModel.Default;
                break;
            }
            case Mini.Core.Games.Services.ContentInstaller.Status.Synchronizing: {
                Status = new StatusModel(message.Status.ToString(), SixIconFont.withSIX_icon_Reload, message.Progress,
                    message.Speed, true,
                    SixColors.SixOrange);
                break;
            }
            case Mini.Core.Games.Services.ContentInstaller.Status.Preparing: {
                Status = new StatusModel(message.Status.ToString(), SixIconFont.withSIX_icon_Cloud, message.Progress,
                    message.Speed, true,
                    SixColors.SixOrange);
                break;
            }
            default: {
                throw new NotSupportedException(message.Status + " is not supported");
            }
            }
        }
    }

    class GameTerminated
    {
        public GameTerminated(Game game, int processId) {
            Game = game;
            ProcessId = processId;
        }

        public Game Game { get; }
        public int ProcessId { get; }
    }

    public class GameStateHandler
    {
        public GameStateHandler(ConcurrentDictionary<Guid, ContentState> states) {
            State = states;
        }

        public ConcurrentDictionary<Guid, ContentState> State { get; }
        public bool IsRunning { get; set; }
    }

    public class StatusModel : IEquatable<StatusModel>
    {
        public static readonly StatusModel Default = new StatusModel(Status.Synchronized.ToString(),
            SixIconFont.withSIX_icon_Hexagon,
            color: SixColors.SixGreen);

        public StatusModel(string text, string icon, double progress = 0, double speed = 0, bool acting = false,
            string color = null) {
            if (progress.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(progress), "NaN");
            if (speed.Equals(double.NaN))
                throw new ArgumentOutOfRangeException(nameof(speed), "NaN");
            if (progress < 0)
                throw new ArgumentOutOfRangeException(nameof(progress), "Below 0");
            if (speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Below 0");
            Text = text;
            Icon = icon;
            Progress = progress;
            Speed = speed;
            Acting = acting;
            Color = color;
        }

        public string Text { get; }
        public string Icon { get; }
        public string Color { get; }
        public double Progress { get; }
        public double Speed { get; }
        public bool Acting { get; }
        // TODO: Merge Acting and State?
        public State State { get; }

        public bool Equals(StatusModel other) {
            return other != null
                   && (ReferenceEquals(this, other)
                       || other.GetHashCode() == GetHashCode());
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Text?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Icon?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Color?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Progress.GetHashCode();
                hashCode = (hashCode*397) ^ Speed.GetHashCode();
                hashCode = (hashCode*397) ^ Acting.GetHashCode();
                hashCode = (hashCode*397) ^ (int) State;
                return hashCode;
            }
        }

        public string ToText() {
            return Text + ": " + ToProgressText();
        }

        public string ToProgressText() {
            return String.Format("{0:0.##} %", Progress) + "@ " + Speed.FormatSpeed();
        }

        public override bool Equals(object obj) {
            return Equals(obj as StatusModel);
        }
    }

    public enum State
    {
        Normal,
        Paused,
        Error
    }
}