// <copyright company="SIX Networks GmbH" file="TrayMainWindowViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.ViewModels.Main.Welcome;
using GetQueue = SN.withSIX.Mini.Applications.Usecases.Main.GetQueue;

namespace SN.withSIX.Mini.Applications.ViewModels.Main
{
    public class TrayMainWindowViewModel : ScreenViewModel, ITrayMainWindowViewModel
    {
        readonly ObservableAsPropertyHelper<Uri> _avatarUrl;
        readonly IReactiveCommand _goAccount;
        readonly IReactiveCommand _goPremium;
        readonly IReactiveCommand _installUpdate;
        readonly TrayMainWindowMenu _menu;
        readonly IReactiveCommand<Unit> _switchQueue;
        readonly ObservableAsPropertyHelper<string> _taskbarToolTip;
        LoginInfo _loginInfo;
        IViewModel _mainArea;
        IStatusViewModel _status;
        AppUpdateState _updateState;

        public TrayMainWindowViewModel(IViewModel mainArea, TrayMainWindowMenu menu, IStatusViewModel status,
            LoginInfo loginInfo, IWelcomeViewModel welcomeViewModel) {
            _mainArea = Consts.FirstRun /* || Cheat.Consts.IsTestVersion */ ? welcomeViewModel : mainArea;
            _menu = menu;
            _status = status;
            _loginInfo = loginInfo;

            welcomeViewModel.Close.Subscribe(x => MainArea = mainArea);

            _taskbarToolTip = this.WhenAnyValue(x => x.DisplayName, x => x.Status, FormatTaskbarToolTip)
                .ToProperty(this, x => x.TitleToolTip);

            _avatarUrl = this.WhenAnyValue<TrayMainWindowViewModel, Uri, AccountInfo>(x => x.LoginInfo.Account,
                x => new Uri("http:" + AvatarCalc.GetAvatarURL(x)))
                .ToProperty(this, x => x.AvatarUrl);

            _installUpdate =
                ReactiveCommand.CreateAsyncTask(
                    this.WhenAnyValue(x => x.UpdateState, state => state == AppUpdateState.UpdateAvailable),
                    async x => await RequestAsync(new OpenWebLink(ViewType.Update)).ConfigureAwait(false))
                    .DefaultSetup("InstallUpdate");
            _goAccount =
                ReactiveCommand.CreateAsyncTask(
                    async x => await RequestAsync(new OpenWebLink(ViewType.Profile)).ConfigureAwait(false))
                    .DefaultSetup("OpenProfile");
            _goPremium =
                ReactiveCommand.CreateAsyncTask(
                    async x =>
                        await
                            RequestAsync(
                                new OpenWebLink(_loginInfo.IsPremium ? ViewType.PremiumAccount : ViewType.GoPremium))
                                .ConfigureAwait(false))
                    .DefaultSetup("GoPremium");

            IViewModel previousMain = null;

            _switchQueue = ReactiveCommand.CreateAsyncTask(
                async x => {
                    if (previousMain == null) {
                        previousMain = _mainArea;
                        MainArea = await RequestAsync(new GetQueue()).ConfigureAwait(false);
                    } else {
                        MainArea = previousMain;
                        previousMain = null;
                    }
                });
            status.SwitchQueue = _switchQueue; // TODO..

            Listen<LoginChanged>()
                .Select(x => x.LoginInfo)
                .ObserveOnMainThread()
                .BindTo(this, x => x.LoginInfo);
            // We need to receive these right away..
            // toDO: Think about how to make this only on WhenActivated
            Listen<AppStateUpdated>()
                .Select(x => x.UpdateState)
                .ObserveOnMainThread()
                .BindTo(this, x => x.UpdateState);
        }

        public Uri AvatarUrl => _avatarUrl.Value;
        public string TitleToolTip => _taskbarToolTip.Value;
        public ICommand InstallUpdate => _installUpdate;
        public ICommand SwitchQueue => _switchQueue;
        public AppUpdateState UpdateState
        {
            get { return _updateState; }
            set { this.RaiseAndSetIfChanged(ref _updateState, value); }
        }

        public string FormatTaskbarToolTip(string s, IStatusViewModel statusViewModel) {
            var baseText = s + " v" + Consts.ProductVersion;
            var statusModel = statusViewModel.Status;
            return statusModel == null
                ? baseText
                : baseText + "\n" +
                  (statusModel.Acting ? statusModel.ToText() : statusModel.Text);
        }

        public IViewModel MainArea
        {
            get { return _mainArea; }
            private set { this.RaiseAndSetIfChanged(ref _mainArea, value); }
        }
        public LoginInfo LoginInfo
        {
            get { return _loginInfo; }
            private set { this.RaiseAndSetIfChanged(ref _loginInfo, value); }
        }
        public ICommand GoAccount => _goAccount;
        public IContextMenu Menu => _menu;
        public IStatusViewModel Status
        {
            get { return _status; }
            private set { this.RaiseAndSetIfChanged(ref _status, value); }
        }
        public ICommand GoPremium => _goPremium;
        public override string DisplayName { get; } = Consts.DisplayTitle;

        public void RemoveUpdatedState() {
            if (UpdateState == AppUpdateState.UpdateInstalled)
                UpdateState = AppUpdateState.Uptodate;
        }
    }

    public interface ITrayMainWindowViewModel : IScreenViewModel
    {
        IContextMenu Menu { get; }
        IStatusViewModel Status { get; }
        ICommand GoPremium { get; }
        LoginInfo LoginInfo { get; }
        ICommand GoAccount { get; }
        ICommand SwitchQueue { get; }
        IViewModel MainArea { get; }
        string TitleToolTip { get; }
        ICommand InstallUpdate { get; }
        AppUpdateState UpdateState { get; }
        Uri AvatarUrl { get; }
        string FormatTaskbarToolTip(string s, IStatusViewModel statusViewModel);
        void RemoveUpdatedState();
    }

    class AvatarCalc
    {
        public static string GetAvatarURL(AccountInfo account) {
            return GetAvatarURL(account, 72);
        }

        public static string GetAvatarURL(AccountInfo account, int size) {
            return account.AvatarURL == null
                ? GetGravatarUrl(account.EmailMd5, size)
                : GetCustomAvatarUrl(account.AvatarURL, account.AvatarUpdatedAt.GetValueOrDefault(0), size);
        }

        public static string GetAvatarURL(string avatarUrl, long? avatarUpdatedAt, string emailMd5, int size = 72) {
            return avatarUrl == null
                ? GetGravatarUrl(emailMd5, size)
                : GetCustomAvatarUrl(avatarUrl, avatarUpdatedAt.GetValueOrDefault(0), size);
        }

        static string GetGravatarUrl(string emailMd5, int size) {
            return "//www.gravatar.com/avatar/" + emailMd5 +
                   "?size=" + size + "&amp;d=%2f%2faz667488.vo.msecnd.net%2fimg%2favatar%2fnoava_" +
                   size + ".jpg";
        }

        static string GetCustomAvatarUrl(string avatarUrl, long avatarUpdatedAt, int size) {
            var v = "?v=" + avatarUpdatedAt;
            return avatarUrl + size + "x" + size + ".jpg" + v;
        }
    }
}