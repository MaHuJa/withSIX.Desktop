// <copyright company="SIX Networks GmbH" file="HomeWorld2SettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using FluentValidation;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class HomeWorld2SettingsDataModel : GameSettingsDataModel
    {
        string _repositoryDirectory;

        public HomeWorld2SettingsDataModel() {
            Validator = new Homeworld2SettingsValidator(Validator);
        }

        [DisplayName(SettingsStrings.RepositoryDirectoryDisplayName)]
        [Category(GameSettingCategories.Directories)]
        [Description(SettingsStrings.RepositoryDirectoryDescription)]
        public string RepositoryDirectory
        {
            get { return _repositoryDirectory; }
            set
            {
                value = CleanPath(value);
                SetProperty(ref _repositoryDirectory, value);
            }
        }

        class Homeworld2SettingsValidator : ChainedValidator<HomeWorld2SettingsDataModel>
        {
            public Homeworld2SettingsValidator(IValidator otherValidator)
                : base(otherValidator) {
                RuleFor(x => x.RepositoryDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage)
                    .Must(BeValidSynqPath)
                    .WithMessage(NotSynqSubPathMessage);
            }
        }
    }
}