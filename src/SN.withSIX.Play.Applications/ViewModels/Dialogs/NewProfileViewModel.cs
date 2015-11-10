// <copyright company="SIX Networks GmbH" file="NewProfileViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentValidation;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Profiles;
using SN.withSIX.Play.Applications.UseCases.Profiles;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Applications.ViewModels.Dialogs
{
    public interface INewProfileViewModel : ISupportsActivation {}

    [DoNotObfuscate]
    public class NewProfileViewModel : DialogBase, IDisposable, INewProfileViewModel
    {
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;
        string _color;
        string _name;
        ProfileDataModel _parentProfile;

        public NewProfileViewModel(IMediator mediator, IDialogManager dialogManager) {
            DisplayName = "New profile";
            _mediator = mediator;
            _dialogManager = dialogManager;

            Validator = new NewProfileValidator();

            ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.IsValid), x => CreateProfile())
                .SetNewCommand(this, x => x.CreateCommand)
                .Subscribe();

            // Must gray out if Creation running...
            ReactiveCommand.Create(CreateCommand.IsExecuting.Select(x => !x))
                .SetNewCommand(this, x => x.CancelCommand).Subscribe(x => TryClose());

            ClearErrors();

            Activator = new ViewModelActivator();

            this.WhenActivated(x => {
                BaseProfiles.ElementAt(0).Name = "Default (global profile)";
                ParentProfile = BaseProfiles.First();
            });
        }

        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ReactiveCommand<Unit> CreateCommand { get; private set; }
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
        public List<string> Colors { get; set; }
        public ProfileDataModel ParentProfile
        {
            get { return _parentProfile; }
            set { SetProperty(ref _parentProfile, value); }
        }
        public IReactiveDerivedList<ProfileDataModel> BaseProfiles { get; set; }

        public void Dispose() {
            Dispose(true);
        }

        public ViewModelActivator Activator { get; }

        async Task CreateProfile() {
            try {
                await _mediator.RequestAsyncWrapped(new CreateProfileCommand(Name, Color, ParentProfile.Id));
                TryClose();
            } catch (ProfileWithSameNameAlreadyExistsException) {
                // TODO: Validation message at name field instead or also?
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(
                        "A profile with that name already exists, please choose a new name",
                        "Profile name already exists"
                        ));
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                BaseProfiles.Dispose();
        }

        class NewProfileValidator : AbstractValidator<NewProfileViewModel>
        {
            const string EmptyNameMessage = "Please specify a name";
            const string TooLongNameMessage = "Name cannot exceed 100 characters";

            public NewProfileValidator() {
                RuleFor(x => x.Name).NotEmpty().WithMessage(EmptyNameMessage);
                RuleFor(x => x.Name).Length(0, 100).WithMessage(TooLongNameMessage);
            }
        }
    }
}