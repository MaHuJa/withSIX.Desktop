// <copyright company="SIX Networks GmbH" file="MiniMainWindowViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.ViewModels.Main;
using SN.withSIX.Mini.Applications.ViewModels.Main.Welcome;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public class MiniMainWindowViewModel : ScreenViewModel, IMiniMainWindowViewModel
    {
        readonly ObservableAsPropertyHelper<string> _taskbarToolTip;

        public MiniMainWindowViewModel(ITrayMainWindowViewModel trayMainWindowViewModel) {
            TrayViewModel = trayMainWindowViewModel;
            _taskbarToolTip = this.WhenAnyValue(x => x.DisplayName, x => x.TrayViewModel.Status,
                trayMainWindowViewModel.FormatTaskbarToolTip)
                .ToProperty(this, x => x.TaskbarToolTip);
            OpenPopup = ReactiveCommand.Create();
            ShowNotification = ReactiveCommand.CreateAsyncTask(async x => (ITrayNotificationViewModel) x);
            Deactivate = ReactiveCommand.Create().DefaultSetup("Deactivate");
            Deactivate.Subscribe(x => {
                if (TrayViewModel.MainArea is IWelcomeViewModel)
                    return;
                Close.Execute(null);
            });

            OpenPopup
                .Take(1)
                .Delay(TimeSpan.FromSeconds(20))
                .ObserveOnMainThread()
                .Subscribe(x => TrayViewModel.RemoveUpdatedState());

            // TODO: Make this a setting?
            /*            Listen<ApiUserActionStarted>()
                .ObserveOnMainThread()
                .InvokeCommand(OpenPopup);*/
            Listen<AppStateUpdated>()
                .Where(x => x.UpdateState == AppUpdateState.Updating)
                .ObserveOnMainThread()
                .InvokeCommand(OpenPopup);
            Listen<ShowTrayNotification>()
                .Select(x => new TrayNotificationViewModel(x.Subject, x.Text, x.CloseIn, x.Actions))
                .ObserveOnMainThread()
                .InvokeCommand(ShowNotification);
        }

        public IReactiveCommand<object> Deactivate { get; }
        public override string DisplayName => Consts.WindowTitle;
        public ITrayMainWindowViewModel TrayViewModel { get; }
        public string TaskbarToolTip => _taskbarToolTip.Value;
        public ReactiveCommand<object> OpenPopup { get; }
        public IReactiveCommand<ITrayNotificationViewModel> ShowNotification { get; }
    }

    public class ShowTrayNotification
    {
        public ShowTrayNotification(string subject, string text, string icon = null, TimeSpan? expirationTime = null,
            params TrayAction[] actions) {
            Subject = subject;
            CloseIn = expirationTime;
            Text = text;
            Icon = icon;
            Actions = actions;
        }

        public string Subject { get; }
        public string Text { get; }
        public string Icon { get; }
        public TimeSpan? CloseIn { get; }
        public ICollection<TrayAction> Actions { get; }
    }

    public interface IMiniMainWindowViewModel : IScreenViewModel
    {
        ITrayMainWindowViewModel TrayViewModel { get; }
        string TaskbarToolTip { get; }
        ReactiveCommand<object> OpenPopup { get; }
        IReactiveCommand<ITrayNotificationViewModel> ShowNotification { get; }
        IReactiveCommand<object> Deactivate { get; }
    }
}