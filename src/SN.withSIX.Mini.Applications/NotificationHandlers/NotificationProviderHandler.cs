// <copyright company="SIX Networks GmbH" file="NotificationProviderHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.ViewModels;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.NotificationHandlers
{
    public class NotificationProviderHandler : DbQueryBase,
        //IAsyncNotificationHandler<ApiUserActionStarted>,
        //IAsyncNotificationHandler<ApiException>,
        //IAsyncNotificationHandler<ApiActionFinished>,
        INotificationHandler<InstallActionCompleted>
    {
        static readonly TimeSpan defaultExpirationTime = TimeSpan.FromSeconds(10);
        readonly IEnumerable<INotificationProvider> _notifiers;

        public NotificationProviderHandler(IDbContextLocator dbContextLocator, IEnumerable<INotificationProvider> notifiers) : base(dbContextLocator) {
            _notifiers = notifiers;
        }

        //public async Task HandleAsync(ApiActionFinished notification) {
        //  Notify("Action", "action finished");
        //}

        //public async Task HandleAsync(ApiException notification) {
        //  Notify("Error", notification.Exception.Message);
        //}

        public void Handle(InstallActionCompleted notification) {
            var info = GetInfo(notification);
            TrayNotify(new ShowTrayNotification("content installed",
                info.Text,
                actions: CreateActions(notification, info), expirationTime: defaultExpirationTime));
        }

        static TrayAction[] CreateActions(InstallActionCompleted notification, ActionInfo info) {
            return
                info.Actions.Select(
                    x => new TrayAction {
                        DisplayName = x.ToString(),
                        Command = CreateCommand(notification, x)
                    }).ToArray();
        }

        static ActionInfo GetInfo(InstallActionCompleted notification) {
            // TODO: Consider more elegant approach to getting the info for the type of content..
            if (notification.Action.Content.Count != 1)
                return DefaultAction(notification);
            var c = notification.Action.Content.First().Content as NetworkCollection;
            // TODO: Consider double action,
            // then need to allow specify the action to execute on a collection,
            // as play currently auto joins if has server etc ;-)
            if (c != null && c.Servers.Any()) {
                return new ActionInfo {
                    Actions = new[] { PlayAction.Join, PlayAction.Launch },
                    Text = "do you wish to join the server of " + notification.Action.Name + "?"
                };
            }
            return DefaultAction(notification);
        }

        static ActionInfo DefaultAction(InstallActionCompleted notification) {
            return new ActionInfo {
                Actions = new[] { PlayAction.Launch },
                Text = "do you want to play " + notification.Action.Name + "?"
            };
        }

        void TrayNotify(ShowTrayNotification showTrayNotification) {
            if (!SettingsContext.Settings.Local.ShowDesktopNotifications)
                return;
            Cheat.MessageBus.SendMessage(showTrayNotification);
        }

        void Notify(string subject, string text) {
            if (!SettingsContext.Settings.Local.ShowDesktopNotifications)
                return;
            foreach (var n in _notifiers) {
                var t = n.Notify(subject, text, expirationTime: defaultExpirationTime); // hmm
            }
        }

        static IReactiveCommand<UnitType> CreateCommand(InstallActionCompleted notification, PlayAction playAction) {
            return ReactiveCommand.CreateAsyncTask(
                async x =>
                    await
                        new LaunchContents(notification.Game.Id,
                            notification.Action.Content.Select(c => new ContentGuidSpec(c.Content.Id)).ToList(),
                            action: playAction.ToLaunchAction()).Execute()
                            .ConfigureAwait(false))
                .DefaultSetup("Play");
        }

        class ActionInfo
        {
            public string Text { get; set; }
            public IReadOnlyCollection<PlayAction> Actions { get; set; }
        }
    }

    public class ApiUserActionFinished : IDomainEvent {}

    public class ApiUserActionStarted : IDomainEvent {}
}