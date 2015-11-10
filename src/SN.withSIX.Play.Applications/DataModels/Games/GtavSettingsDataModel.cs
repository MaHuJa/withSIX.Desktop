// <copyright company="SIX Networks GmbH" file="GtavSettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using FluentValidation;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class GTAVSettingsDataModel : GameSettingsDataModel
    {
        string _repositoryDirectory;

        public GTAVSettingsDataModel() {
            Validator = new GTAVSettingsValidator(Validator);
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

        class GTAVSettingsValidator : ChainedValidator<GTAVSettingsDataModel>
        {
            public GTAVSettingsValidator(IValidator otherValidator)
                : base(otherValidator) {
                RuleFor(x => x.RepositoryDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage)
                    .Must(BeValidSynqPath)
                    .WithMessage(NotSynqSubPathMessage);
            }
        }
    }
}