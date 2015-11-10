// <copyright company="SIX Networks GmbH" file="GameSettingsTabViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Windows.Input;
using FluentValidation;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public interface IGameSettingsTabViewModel : ISettingsTabViewModel, IHaveId<Guid>
    {
        ICommand ToggleStartupParameters { get; }
        string GameDirectory { get; set; }
        string RepoDirectory { get; set; }
        GameStartupParameters StartupParameters { get; set; }
        bool ShowStartupParameters { get; set; }
    }

    public interface IGameSettingsApiModel : IHaveId<Guid>
    {
        string GameDirectory { get; set; }
        string RepoDirectory { get; set; }
        string StartupLine { get; set; }
    }

    public interface IGameSettingsWithConfigurablePackageDirectoryTabViewModel : IGameSettingsTabViewModel
    {
        string PackageDirectory { get; set; }
    }

    public abstract class GameSettingsApiModel : IGameSettingsApiModel
    {
        public Guid Id { get; set; }
        public string GameDirectory { get; set; }
        public string RepoDirectory { get; set; }
        public string StartupLine { get; set; }
    }

    public abstract class GameSettingsWithConfigurablePackageApiModel : GameSettingsApiModel
    {
        public string PackageDirectory { get; set; }
    }

    public abstract class GameSettingsTabViewModel : SettingsTabViewModel, IGameSettingsTabViewModel
    {
        readonly IReactiveCommand<object> _toggleStartupParameters;
        string _gameDirectory;
        string _repoDirectory;
        bool _showStartupParameters;
        GameStartupParameters _startupParameters;

        protected GameSettingsTabViewModel() {
            _toggleStartupParameters = ReactiveCommand.Create();
            _toggleStartupParameters.Subscribe(x => ShowStartupParameters = !ShowStartupParameters);

            Validator = new GameSettingsValidator();

            // TODO: Or build into SetProperty?
            this.WhenAnyValue(x => x.RepoDirectory, x => x.GameDirectory)
                .Subscribe(x => UpdateValidation());
        }

        public GameStartupParameters StartupParameters
        {
            get { return _startupParameters; }
            set { this.RaiseAndSetIfChanged(ref _startupParameters, value); }
        }
        public bool ShowStartupParameters
        {
            get { return _showStartupParameters; }
            set { this.RaiseAndSetIfChanged(ref _showStartupParameters, value); }
        }
        public Guid Id { get; set; }
        public ICommand ToggleStartupParameters => _toggleStartupParameters;
        // TODO: Path Validation!
        public string GameDirectory
        {
            get { return _gameDirectory; }
            set { this.RaiseAndSetIfChanged(ref _gameDirectory, value); }
        }
        // TODO: Path Validation!
        public string RepoDirectory
        {
            get { return _repoDirectory; }
            set { this.RaiseAndSetIfChanged(ref _repoDirectory, value); }
        }

        protected class GameSettingsValidator : ValidatorBase<GameSettingsTabViewModel>
        {
            protected internal const string NotSynqSubPathMessage =
                "Please specify a path which is not a subdirectory under any .synq directory";

            public GameSettingsValidator() {
                RuleFor(x => x.GameDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage);
                RuleFor(x => x.RepoDirectory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage)
                    .Must(BeValidSynqPath)
                    .WithMessage(NotSynqSubPathMessage);
            }

            static bool BeValidSynqPath(string synqPath) {
                if (string.IsNullOrWhiteSpace(synqPath))
                    return true;

                return Tools.FileUtil.FindParentWithName(synqPath, Repository.DefaultRepoRootDirectory) ==
                       null;
            }
        }
    }

    public abstract class GameSettingsWithConfigurablePackageDirectoryTabViewModel : GameSettingsTabViewModel
    {
        string _packageDirectory;

        protected GameSettingsWithConfigurablePackageDirectoryTabViewModel() {
            Validator = new GameSettingsWithConfigurablePackageDirectoryTabViewModelValidator(Validator);
            // TODO: Or build into SetProperty?
            this.WhenAnyValue(x => x.PackageDirectory)
                .Subscribe(x => UpdateValidation());
        }

        public string PackageDirectory
        {
            get { return _packageDirectory; }
            set { this.RaiseAndSetIfChanged(ref _packageDirectory, value); }
        }

        class GameSettingsWithConfigurablePackageDirectoryTabViewModelValidator :
            ChainedValidator<GameSettingsWithConfigurablePackageDirectoryTabViewModel>
        {
            public GameSettingsWithConfigurablePackageDirectoryTabViewModelValidator(IValidator otherValidator)
                : base(otherValidator) {
                RuleFor(x => x.PackageDirectory)
                    .Must((model, x) => BeValidModPath(model.RepoDirectory, x))
                    .WithMessage(GameSettingsValidator.NotSynqSubPathMessage);
            }

            static bool BeValidModPath(string repositoryDirectory, string modPath) {
                if (string.IsNullOrWhiteSpace(modPath))
                    return true;

                var parentCheck = Tools.FileUtil.FindParentWithName(modPath, Repository.DefaultRepoRootDirectory) ==
                                  null;

                if (Path.GetFileName(modPath) == Repository.DefaultRepoRootDirectory)
                    return false;

                if (string.IsNullOrWhiteSpace(repositoryDirectory))
                    return parentCheck;

                var synqRepoDirectory = Path.Combine(repositoryDirectory, Repository.DefaultRepoRootDirectory);
                return !Tools.FileUtil.ComparePathsOsCaseSensitive(synqRepoDirectory, modPath)
                       && parentCheck;
            }
        }
    }
}