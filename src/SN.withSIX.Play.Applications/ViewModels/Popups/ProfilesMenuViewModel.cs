// <copyright company="SIX Networks GmbH" file="ProfilesMenuViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Profiles;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Profiles;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Popups
{
    [DoNotObfuscate]
    public class ProfilesMenuViewModel : PopupBase, IDisposable
    {
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;
        ProfileDataModel _activeProfile;
        bool _showProfilesMenu;

        public ProfilesMenuViewModel(IMediator mediator, IDialogManager dialogManager) {
            _mediator = mediator;
            _dialogManager = dialogManager;

            this.SetCommand(x => x.ProfilesMenuCommand).Subscribe(x => ShowProfilesMenu = !ShowProfilesMenu);
            ReactiveUI.ReactiveCommand.CreateAsyncTask(x => OpenNewProfileDialog())
                .SetNewCommand(this, x => x.AddNewProfileCommand)
                .Subscribe();
        }

        public ReactiveCommand ProfilesMenuCommand { get; private set; }
        public ReactiveCommand<Unit> AddNewProfileCommand { get; private set; }
        public bool ShowProfilesMenu
        {
            get { return _showProfilesMenu; }
            set { SetProperty(ref _showProfilesMenu, value); }
        }
        public IReactiveDerivedList<ProfileDataModel> Profiles { get; internal set; }
        public ProfileDataModel ActiveProfile
        {
            get { return _activeProfile; }
            set { SetProperty(ref _activeProfile, value); }
        }

        public void Dispose() {
            Dispose(true);
        }

        [DoNotObfuscate]
        public void DeleteProfile(ProfileDataModel profile) {
            ShowProfilesMenu = false;
            var result = _dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                String.Format("Are you sure you want to delete the profile '{0}'?", profile.Name),
                "Confirm delete profile", SixMessageBoxButton.YesNo));

            if (result.IsYes())
                _mediator.Request(new DeleteProfileCommand(profile.Id));
        }

        [DoNotObfuscate]
        public void SwitchProfile(ProfileDataModel profile) {
            ShowProfilesMenu = false;
            _mediator.Request(new SwitchProfileCommand(profile.Id));
        }

        async Task OpenNewProfileDialog() {
            ShowProfilesMenu = false;
            using (var vm = _mediator.Request(new ShowNewProfileDialogQuery()))
                await _dialogManager.ShowDialogAsync(vm).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                Profiles.Dispose();
        }
    }
}