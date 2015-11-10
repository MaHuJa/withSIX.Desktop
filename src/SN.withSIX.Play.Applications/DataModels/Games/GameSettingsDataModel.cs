// <copyright company="SIX Networks GmbH" file="GameSettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using FluentValidation;
using FluentValidation.Results;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class GameSettingsDataModel : DataModel
    {
        string _defaultDirectory;
        string _directory;
        protected bool _injectSteam;
        bool _launchAsAdministrator;
        ProcessPriorityClass _priority;
        GameStartupParameters _startupParameters;

        public GameSettingsDataModel() {
            Validator = new GameSettingsValidator();

            this.WhenAnyValue(x => x.DefaultDirectory)
                .Where(x => string.IsNullOrWhiteSpace(Directory) && !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => { Directory = x; });
        }

        [Browsable(false)]
        public Guid GameId { get; set; }
        [Category(GameSettingCategories.Launching)]
        [DisplayName("Force 'Run as Administrator'")]
        [Description("Forces running the Game as Administrator")]
        public bool LaunchAsAdministrator
        {
            get { return _launchAsAdministrator; }
            set { SetProperty(ref _launchAsAdministrator, value); }
        }
        [Category(GameSettingCategories.Launching)]
        [DisplayName("Use Steam In-Game")]
        [Description("Add the Steam In-Game Overlay to your game (Even if it does not support Steam!)")]
        public bool InjectSteam
        {
            get { return _injectSteam; }
            set { SetProperty(ref _injectSteam, value); }
        }
        [Category(GameSettingCategories.Launching)]
        [DisplayName("Priority")]
        [Description("Launch the game with desired priority")]
        public ProcessPriorityClass Priority
        {
            get { return _priority; }
            set { SetProperty(ref _priority, value); }
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
                value = CleanPath(value);
                SetProperty(ref _directory, value);
            }
        }
        [Browsable(false)]
        public string DefaultDirectory
        {
            get { return _defaultDirectory; }
            set { SetProperty(ref _defaultDirectory, value); }
        }
        [Description("Open the Editor for more advanced options")]
        [DisplayName("Startup Parameters")]
        [Category(GameSettingCategories.Launching)]
        public GameStartupParameters StartupParameters
        {
            get { return _startupParameters; }
            set { SetProperty(ref _startupParameters, value); }
        }

        internal static string CleanPath(string value) {
            if (value != null)
                value = value.Trim();
            if (string.IsNullOrWhiteSpace(value))
                value = null;
            return value;
        }

        internal abstract class ChainedValidator<T> : ValidatorBase<T>
        {
            readonly IValidator _otherValidator;

            protected ChainedValidator(IValidator otherValidator) {
                _otherValidator = otherValidator;
            }

            public override ValidationResult Validate(T instance) {
                var otherResult = _otherValidator.Validate(instance);

                var myResult = base.Validate(instance);
                foreach (var v in myResult.Errors)
                    otherResult.Errors.Add(v);

                return otherResult;
            }
        }

        class GameSettingsValidator : ValidatorBase<GameSettingsDataModel>
        {
            public GameSettingsValidator() {
                RuleFor(x => x.Directory)
                    .Must(BeValidPath).WithMessage(ValidPathMessage);
            }
        }

        internal static class SettingsStrings
        {
            public const string RepositoryDirectoryDisplayName = "Synq (cache) directory";
            public const string RepositoryDirectoryDescription = "(Optional) Defaults to Mod directory";
        }

        internal class ValidatorBase<T> : AbstractValidator<T>
        {
            protected const string ValidPathMessage = "Please specify a valid path";
            protected const string NotSynqSubPathMessage =
                "Please specify a path which is not a subdirectory under any .synq directory";
            protected const string ValidOptionalPathsMessage = "Please specify valid paths separated by ;";

            protected static bool BeValidSynqPath(string synqPath) {
                if (string.IsNullOrWhiteSpace(synqPath))
                    return true;

                return Tools.FileUtil.FindParentWithName(synqPath, Repository.DefaultRepoRootDirectory) ==
                       null;
            }

            protected static bool BeValidPath(string path) {
                return Tools.FileUtil.IsValidRootedPath(path);
            }

            protected static bool BeValidModPath(string repositoryDirectory, string modPath) {
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

            protected static bool BeValidOptionalPaths(string paths) {
                return string.IsNullOrWhiteSpace(paths) ||
                       paths.Split(';').All(x => Tools.FileUtil.IsValidRootedPath(x));
            }
        }
    }
}