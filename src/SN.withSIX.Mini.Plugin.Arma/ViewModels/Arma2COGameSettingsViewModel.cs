// <copyright company="SIX Networks GmbH" file="Arma2COGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI;

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface IArma2COGameSettingsViewModel : IRealVirtualityGameSettingsViewModel
    {
        string Arma2GameDirectory { get; set; }
    }

    public class Arma2COGameSettingsViewModel : RealVirtualityGameSettingsViewModel, IArma2COGameSettingsViewModel
    {
        string _arma2GameDirectory;

        public Arma2COGameSettingsViewModel() {
            // TODO: Validate the arma2 path etc..
            //Validator = new GameSettingsValidator();

            // TODO: Or build into SetProperty?
            this.WhenAnyValue(x => x.Arma2GameDirectory)
                .Subscribe(x => UpdateValidation());
        }

        public override string DisplayName { get; } = "Arma 2 Combined Operations";
        public string Arma2GameDirectory
        {
            get { return _arma2GameDirectory; }
            set { this.RaiseAndSetIfChanged(ref _arma2GameDirectory, value); }
        }
    }

    public interface IArma2OaGameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class Arma2OaGameSettingsViewModel : RealVirtualityGameSettingsViewModel, IArma2OaGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Arma 2 Operation Arrowhead";
    }

    public interface IArma2GameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class Arma2GameSettingsViewModel : RealVirtualityGameSettingsViewModel, IArma2GameSettingsViewModel
    {
        public override string DisplayName { get; } = "Arma 2 Original";
    }

    public interface IArma1GameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class Arma1GameSettingsViewModel : RealVirtualityGameSettingsViewModel, IArma1GameSettingsViewModel
    {
        public override string DisplayName { get; } = "Arma 1";
    }
}