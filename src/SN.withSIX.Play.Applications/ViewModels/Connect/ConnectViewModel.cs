// <copyright company="SIX Networks GmbH" file="ConnectViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MoreLinq;
using ReactiveUI;
using ReactiveUI.Legacy;
using ShortBus;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Filters;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    [DoNotObfuscate]
    public class ConnectViewModel : ScreenBase<IPlayShellViewModel>, IHandle<ApiKeyUpdated>, IDropTarget
    {
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly UserSettings _settings;
        bool _isEnabled;
        bool _isProfileShown;
        bool _showContactsMenu;
        bool _wasEnabled;

        public ConnectViewModel(ContactList contactList, IDialogManager dialogManager,
            ModsViewModel mods, MissionsViewModel missions, UserSettings settings,
            IEventAggregator ea) {
            _dialogManager = dialogManager;
            _settings = settings;
            _eventBus = ea;
            ContactList = contactList;
            Mods = mods;
            Missions = missions;

            _wasEnabled = IsEnabled;
            IsEnabled = false;
        }

        protected ConnectViewModel() {}
        public ReactiveCommand SwitchShowAll { get; private set; }
        public ReactiveCommand SwitchShowOnlyOnline { get; private set; }
        public ReactiveCommand SwitchShowOnlyIngame { get; private set; }
        public ReactiveCommand LogoutCommand { get; private set; }
        public bool ShowContactsMenu
        {
            get { return _showContactsMenu; }
            set { SetProperty(ref _showContactsMenu, value); }
        }
        public bool IsProfileShown
        {
            get { return _isProfileShown; }
            set { SetProperty(ref _isProfileShown, value); }
        }
        public ContactList ContactList { get; set; }
        public ModsViewModel Mods { get; set; }
        public WebBrowser ApiBrowser { get; set; }
        public ReactiveCommand VisitProfileCommand { get; private set; }
        public ReactiveCommand EditProfileCommand { get; private set; }
        public ReactiveCommand RetryConnectionCommand { get; private set; }
        public ReactiveCommand LoginCommand { get; private set; }
        public ReactiveCommand ContactsMenuCommand { get; private set; }
        public ReactiveCommand RegisterCommand { get; private set; }
        public ReactiveCommand FindFriendCommand { get; protected set; }
        public ReactiveCommand NewGroupCommand { get; protected set; }
        public ReactiveCommand ResetHiddenInviteRequests { get; protected set; }
        public ReactiveCommand OChat { get; private set; }
        public ReactiveCommand ApproveCommand { get; private set; }
        public ReactiveCommand DeclineCommand { get; private set; }
        public ReactiveCommand HideCommand { get; private set; }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        public MissionsViewModel Missions { get; private set; }

        public void DragOver(IDropInfo dropInfo) {
            return;
        }

        public void Drop(IDropInfo dropInfo) {
            return;
        }

        #region IHandle events

        public void Handle(ApiKeyUpdated message) {
            if (string.IsNullOrWhiteSpace(message.ApiKey))
                IsEnabled = false;
        }

        #endregion

        protected override void OnInitialize() {
            base.OnInitialize();

            this.WhenAnyValue(x => x.IsEnabled).Where(x => x).Subscribe(x => _wasEnabled = x);

            this.WhenAnyValue(x => x.ContactList.OnlineStatus)
                .Subscribe(x => IsProfileShown = false);
            this.SetCommand(x => x.RegisterCommand).Subscribe(x => Register());
            this.SetCommand(x => x.LoginCommand).Subscribe(x => Login());
            this.SetCommand(x => x.RetryConnectionCommand).RegisterAsyncTask(ContactList.HandleConnection).Subscribe();
            this.SetCommand(x => x.EditProfileCommand).Subscribe(x => ShowEditProfile());
            this.SetCommand(x => x.LogoutCommand).RegisterAsyncTask(x => Task.Run(() => Logout())).Subscribe();


            this.WhenAnyValue(x => x.ContactList.LoginState, x => x.ContactList.ConnectedState,
                (loggedIn, connected) => new {LoggedIn = loggedIn, Connected = connected})
                .Subscribe(value => LoginOrConnectedStateChanged(value.LoggedIn, value.Connected));
        }

        void LoginOrConnectedStateChanged(LoginState loginState, ConnectedState connectedState) {
            if (connectedState == ConnectedState.ConnectingFailed || connectedState == ConnectedState.Disconnected ||
                loginState == LoginState.LoggedOut || loginState == LoginState.InvalidLogin ||
                connectedState == ConnectedState.Connecting)
                IsEnabled = false;

            if (connectedState == ConnectedState.Connected && loginState == LoginState.LoggedIn)
                IsEnabled = _wasEnabled;
        }

        [DoNotObfuscate]
        public void SwitchProfileShown() {
            IsProfileShown = !IsProfileShown;
        }

        public void ExecuteJS(string function, params object[] pars) {
            ApiBrowser.InvokeScript(function, pars);
        }

        [SmartAssembly.Attributes.ReportUsage]
        void Login() {
            ContactList.RetrieveApiKey();
        }

        [SmartAssembly.Attributes.ReportUsage]
        void Register() {
            _eventBus.PublishOnCurrentThread(new RequestOpenBrowser(CommonUrls.RegisterUrl));
        }

        [SmartAssembly.Attributes.ReportUsage]
        void ShowEditProfile() {
            BrowserHelper.TryOpenUrlIntegrated(CommonUrls.AccountSettingsUrl);
            IsProfileShown = false;
        }

        void Logout() {
            if (String.IsNullOrWhiteSpace(_settings.AccountOptions.AccessToken)) {
                UsageCounter.ReportUsage("Dialog - Appear logged out");
                _dialogManager.MessageBoxSync(new MessageBoxDialogParams("You already appear logged out"));
                Common.App.PublishEvent(new DoLogout());
                return;
            }

            if (!_settings.AppOptions.RememberWarnOnLogout) {
                var r =
                    _dialogManager.MessageBoxSync(new MessageBoxDialogParams("Do you want to log out?", "Are you sure?",
                        SixMessageBoxButton.YesNo) {RememberedState = false});

                if (r == SixMessageBoxResult.YesRemember) {
                    _settings.AppOptions.RememberWarnOnLogout = true;
                    _settings.AppOptions.WarnOnLogout = true;
                    UsageCounter.ReportUsage("Dialog - Remember Warn On Logout");
                } else if (r == SixMessageBoxResult.NoRemember) {
                    _settings.AppOptions.RememberWarnOnLogout = true;
                    _settings.AppOptions.WarnOnLogout = false;
                    UsageCounter.ReportUsage("Dialog - Don't remember Warn On Logout");
                }

                if (r.IsYes())
                    DoLogout();
            } else {
                if (_settings.AppOptions.WarnOnLogout)
                    DoLogout();
            }
        }

        void DoLogout() {
            _settings.AccountOptions.AccessToken = null;
            IsProfileShown = false;
            Common.App.PublishEvent(new DoLogout());
            //            Common.App.PublishEvent(new RequestOpenBrowser(CommonUrls.LogoutUrl));
        }
    }

    public class DoLogout {}
}