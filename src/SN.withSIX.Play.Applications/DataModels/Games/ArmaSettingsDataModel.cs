// <copyright company="SIX Networks GmbH" file="ArmaSettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using FluentValidation;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class ArmaSettingsDataModel : RealVirtualityGameSettingsDataModel
    {
        string _additionalMods;
        string _defaultModDirectory;
        bool _includeServerMods;
        int? _keepLatestVersions;
        string _modDirectory;
        string _repositoryDirectory;
        bool _serverMode;

        public ArmaSettingsDataModel() {
            Validator = new ArmaSettingsValidator(Validator);

            this.WhenAnyValue(x => x.DefaultModDirectory)
                .Where(x => string.IsNullOrWhiteSpace(ModDirectory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { ModDirectory = x; });

            this.WhenAnyValue(x => x.ModDirectory)
                .Where(x => string.IsNullOrWhiteSpace(RepositoryDirectory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { RepositoryDirectory = x; });
        }

        [Browsable(false)]
        public string DefaultModDirectory
        {
            get { return _defaultModDirectory; }
            set { SetProperty(ref _defaultModDirectory, value); }
        }
        [DisplayName("Additional mods")]
        [Category(GameSettingCategories.Directories)]
        [Description("(Optional) Additional mod directories to load, separate paths by ;")]
        public string AdditionalMods
        {
            get { return _additionalMods; }
            set { SetProperty(ref _additionalMods, value); }
        }
        [DisplayName("Mods directory")]
        [Category(GameSettingCategories.Directories)]
        [Description("Where should the modfolders be installed")]
        public string ModDirectory
        {
            get { return _modDirectory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = DefaultModDirectory;
                value = CleanPath(value);
                SetProperty(ref _modDirectory, value);
            }
        }
        [DisplayName("Keep latest content versions")]
        [Category(GameSettingCategories.Advanced)]
        [Description("For each mod how many historical versions should be kept?")]
        public int? KeepLatestVersions
        {
            get { return _keepLatestVersions; }
            set { SetProperty(ref _keepLatestVersions, value); }
        }
        [DisplayName(SettingsStrings.RepositoryDirectoryDisplayName)]
        [Category(GameSettingCategories.Directories)]
        [Description(SettingsStrings.RepositoryDirectoryDescription)]
        public string RepositoryDirectory
        {
            get { return _repositoryDirectory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = ModDirectory;
                value = CleanPath(value);
                SetProperty(ref _repositoryDirectory, value);
            }
        }
        [DisplayName("Load server mods")]
        [Category(GameSettingCategories.Game)]
        [Description(
            "If a server you are to join requires additional mods, should the missing mods get downloaded/activated automatically?"
            )]
        public bool IncludeServerMods
        {
            get { return _includeServerMods; }
            set { SetProperty(ref _includeServerMods, value); }
        }
        [DisplayName("Launch as dedicated server")]
        [Category(GameSettingCategories.Launching)]
        [Description("Runs Game in the dedicated server mode instead of the regular client mode for playing.")]
        public bool ServerMode
        {
            get { return _serverMode; }
            set { SetProperty(ref _serverMode, value); }
        }

        class ArmaSettingsValidator : ChainedValidator<ArmaSettingsDataModel>
        {
            public ArmaSettingsValidator(IValidator otherValidator) : base(otherValidator) {
                RuleFor(x => x.AdditionalMods)
                    .Must(BeValidOptionalPaths).WithMessage(ValidOptionalPathsMessage);
                RuleFor(x => x.ModDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage)
                    .Must((model, value) => BeValidModPath(model.RepositoryDirectory, value))
                    .WithMessage(NotSynqSubPathMessage);
                RuleFor(x => x.RepositoryDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage)
                    .Must(BeValidSynqPath)
                    .WithMessage(NotSynqSubPathMessage);
            }
        }
    }

    public class Arma2OaSettingsDataModel : ArmaSettingsDataModel
    {
        ServerQueryMode _serverQueryMode;
        [DisplayName("Server query protocol preference")]
        [Category(GameSettingCategories.Server)]
        [Description("Which server query protocol would you like to use")]
        public ServerQueryMode ServerQueryMode
        {
            get { return _serverQueryMode; }
            set { SetProperty(ref _serverQueryMode, value); }
        }
    }

    public class Arma2CoSettingsDataModel : Arma2OaSettingsDataModel
    {
        public Arma2CoSettingsDataModel() {
            Arma2Original = new Arma2OriginalChildSettingsDataModel("ARMA 2");
            Arma2Free = new Arma2OriginalChildSettingsDataModel("ARMA 2 Free");

            Arma2Original.Changed.Subscribe(x => OnPropertyChanged("Arma2Original"));
            Arma2Free.Changed.Subscribe(x => OnPropertyChanged("Arma2Free"));
        }

        [ExpandableObject]
        [Category(GameSettingCategories.DirectoriesCombined)]
        [DisplayName("ARMA 2")]
        [Description(
            "Either ARMA2 Original or Free is required to combine with Operation Arrowhead. (or must be integrated)")]
        public Arma2OriginalChildSettingsDataModel Arma2Original { get; }
        [ExpandableObject]
        [Category(GameSettingCategories.DirectoriesCombined)]
        [DisplayName("ARMA 2 Free")]
        [Description(
            "Either ARMA2 Original or Free is required to combine with Operation Arrowhead. (or must be integrated)")]
        public Arma2OriginalChildSettingsDataModel Arma2Free { get; }
    }

    public class Arma2OriginalChildSettingsDataModel : DataModel
    {
        readonly string _game;
        string _defaultDirectory;
        string _defaultModDirectory;
        string _directory;
        string _modDirectory;
        string _repositoryDirectory;

        public Arma2OriginalChildSettingsDataModel() {
            Validator = new ChildSettingsValidator();

            this.WhenAnyValue(x => x.DefaultDirectory)
                .Where(x => string.IsNullOrWhiteSpace(Directory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { Directory = x; });

            this.WhenAnyValue(x => x.DefaultModDirectory)
                .Where(x => string.IsNullOrWhiteSpace(ModDirectory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { ModDirectory = x; });

            this.WhenAnyValue(x => x.ModDirectory)
                .Where(x => string.IsNullOrWhiteSpace(RepositoryDirectory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { RepositoryDirectory = x; });
        }

        public Arma2OriginalChildSettingsDataModel(string game) {
            _game = game;
        }

        [Browsable(false)]
        public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changing
        {
            get { return base.Changing; }
        }
        [Browsable(false)]
        public new IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changed
        {
            get { return base.Changed; }
        }
        [Browsable(false)]
        public new IObservable<Exception> ThrownExceptions
        {
            get { return base.ThrownExceptions; }
        }
        [Browsable(false)]
        public string DefaultDirectory
        {
            get { return _defaultDirectory; }
            set { SetProperty(ref _defaultDirectory, value); }
        }
        [DisplayName("Game directory")]
        [Category(GameSettingCategories.Directories)]
        [Description("Where is the game installed")]
        public string Directory
        {
            get { return _directory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = DefaultDirectory;
                value = GameSettingsDataModel.CleanPath(value);
                SetProperty(ref _directory, value);
            }
        }
        [Browsable(false)]
        public string DefaultModDirectory
        {
            get { return _defaultModDirectory; }
            set { SetProperty(ref _defaultModDirectory, value); }
        }
        [DisplayName("Mods directory")]
        [Category(GameSettingCategories.Directories)]
        [Description("Where should the modfolders be installed")]
        public string ModDirectory
        {
            get { return _modDirectory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = DefaultModDirectory;
                value = GameSettingsDataModel.CleanPath(value);
                SetProperty(ref _modDirectory, value);
            }
        }
        [DisplayName(GameSettingsDataModel.SettingsStrings.RepositoryDirectoryDisplayName)]
        [Category(GameSettingCategories.Directories)]
        [Description(GameSettingsDataModel.SettingsStrings.RepositoryDirectoryDescription)]
        public string RepositoryDirectory
        {
            get { return _repositoryDirectory; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = ModDirectory;
                value = GameSettingsDataModel.CleanPath(value);
                SetProperty(ref _repositoryDirectory, value);
            }
        }

        public override string ToString() {
            return _game;
        }
    }

    class ChildSettingsValidator : GameSettingsDataModel.ValidatorBase<Arma2OriginalChildSettingsDataModel>
    {
        public ChildSettingsValidator() {
            RuleFor(x => x.Directory)
                .Must(BeValidPath).WithMessage(ValidPathMessage);
            RuleFor(x => x.ModDirectory)
                .Must(BeValidPath).WithMessage(ValidPathMessage)
                .Must((model, value) => BeValidModPath(model.RepositoryDirectory, value))
                .WithMessage(NotSynqSubPathMessage);
            RuleFor(x => x.RepositoryDirectory)
                .Must(BeValidPath).WithMessage(ValidPathMessage)
                .Must(BeValidSynqPath)
                .WithMessage(NotSynqSubPathMessage);
        }
    }
}