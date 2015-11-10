// <copyright company="SIX Networks GmbH" file="AccountSettingsTabViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Annotations;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Main;
using SN.withSIX.Mini.Applications.Usecases.Settings;
using SN.withSIX.Mini.Applications.ViewModels.Login;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    [Order(2)]
    public class AccountSettingsTabViewModel : SettingsTabViewModel, IAccountSettingsTabViewModel
    {
        readonly IReactiveCommand<UnitType> _goPremium;
        readonly ReactiveCommand<ILoginViewModel> _login;
        readonly IReactiveCommand _logout;
        LoginInfo _loginInfo;

        public AccountSettingsTabViewModel() {
            _login = ReactiveCommand.CreateAsyncTask(
                async x => await OpenScreenCached(new GetLogin()).ConfigureAwait(false))
                .DefaultSetup("Login");

            _logout =
                ReactiveCommand.CreateAsyncTask(
                    async x => await RequestAsync(new LogoutCommand()).ConfigureAwait(false))
                    .DefaultSetup("Logout");

            _goPremium =
                ReactiveCommand.CreateAsyncTask(
                    async x =>
                        await
                            (_loginInfo.IsPremium
                                ? RequestAsync(new OpenWebLink(ViewType.PremiumAccount))
                                : RequestAsync(new OpenWebLink(ViewType.GoPremium))).ConfigureAwait(false))
                    .DefaultSetup("GoPremium");

            this.WhenActivated(d => {
                d(Listen<LoginChanged>()
                    .Select(x => x.LoginInfo)
                    .ObserveOnMainThread()
                    .BindTo(this, x => x.LoginInfo));
            });
        }

        public override string DisplayName => "Account";
        public LoginInfo LoginInfo
        {
            get { return _loginInfo; }
            private set { this.RaiseAndSetIfChanged(ref _loginInfo, value); }
        }
        public ICommand Login => _login;
        public ICommand GoPremium => _goPremium;
        public ICommand Logout => _logout;
    }

    public interface IAccountSettingsTabViewModel : IAppSettingsViewModel
    {
        ICommand GoPremium { get; }
        ICommand Logout { get; }
        ICommand Login { get; }
        LoginInfo LoginInfo { get; }
    }

    public interface IAppSettingsViewModel : ISettingsTabViewModel {}
}