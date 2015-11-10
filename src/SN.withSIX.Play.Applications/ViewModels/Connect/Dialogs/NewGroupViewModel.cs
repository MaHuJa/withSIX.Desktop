// <copyright company="SIX Networks GmbH" file="NewGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FluentValidation;
using NDepend.Path;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Social;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Validators;
using SN.withSIX.Play.Applications.UseCases.Groups;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Connect.Dialogs
{
    public interface INewGroupViewModel {}

    [DoNotObfuscate]
    public class NewGroupViewModel : DialogBase, INewGroupViewModel
    {
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;
        string _backgroundFilename;
        string _description;
        string _logoFilename;
        string _name;
        string _url;

        public NewGroupViewModel(IMediator mediator, IDialogManager dialogManager) {
            DisplayName = "Create a new group";
            _mediator = mediator;
            _dialogManager = dialogManager;

            Validator = new NewGroupValidator();

            this.SetCommand(x => x.BrowseLogoCommand).Subscribe(x => LogoFilename = _dialogManager.BrowseForFile());
            this.SetCommand(x => x.BrowseBackgroundCommand)
                .Subscribe(x => BackgroundFilename = _dialogManager.BrowseForFile());
            this.SetCommand(x => x.CancelCommand).Subscribe(x => TryClose());
            this.SetCommand(x => x.CreateCommand, this.WhenAnyValue(x => x.IsValid))
                .RegisterAsyncTask(CreateGroup)
                .Subscribe();

            ClearErrors();
        }

        public ReactiveCommand BrowseLogoCommand { get; protected set; }
        public ReactiveCommand BrowseBackgroundCommand { get; protected set; }
        public ReactiveCommand CancelCommand { get; protected set; }
        public ReactiveCommand CreateCommand { get; protected set; }
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string URL
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        public string LogoFilename
        {
            get { return _logoFilename; }
            set { SetProperty(ref _logoFilename, value); }
        }
        public string BackgroundFilename
        {
            get { return _backgroundFilename; }
            set { SetProperty(ref _backgroundFilename, value); }
        }

        async Task CreateGroup() {
            try {
                await
                    _mediator.RequestAsyncWrapped(new CreateGroupCommand(Name, Description,
                        string.IsNullOrWhiteSpace(URL)
                            ? null
                            : new Uri(URL.StartsWith("http://") || URL.StartsWith("https://") ? URL : "http://" + URL),
                        LogoFilename == null ? null : LogoFilename.ToAbsoluteFilePath(),
                        BackgroundFilename == null ? null : BackgroundFilename.ToAbsoluteFilePath()));
                TryClose();
            } catch (GroupAlreadyExistsException ex) {
                MainLog.Logger.FormattedErrorException(ex);
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams("The group name specified already exists, please choose another one.",
                        "Error Creating Group", SixMessageBoxButton.OK));
            } catch (Exception ex) {
                MainLog.Logger.FormattedErrorException(ex);
                _dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams("We were unable to create your group:\n" + ex.Message,
                        "Error Creating Group", SixMessageBoxButton.OK));
            }
        }

        class NewGroupValidator : AbstractValidator<NewGroupViewModel>
        {
            const string ValidNameMessage = "Please specify a name";

            public NewGroupValidator() {
                RuleFor(x => x.Name).NotEmpty().WithMessage(ValidNameMessage);
                RuleFor(x => x.URL).Must(x => UriValidator.IsValidUriWithHttpFallback(x, "http", "https"))
                    .WithMessage("Should be valid http(s) link");
            }
        }
    }
}