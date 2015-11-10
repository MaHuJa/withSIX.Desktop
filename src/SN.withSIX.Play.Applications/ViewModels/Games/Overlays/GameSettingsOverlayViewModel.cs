// <copyright company="SIX Networks GmbH" file="GameSettingsOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class GameSettingsOverlayViewModel : OverlayViewModelBase
    {
        const string BikiUrl = "https://community.bistudio.com/wiki/Arma2:_Startup_Parameters";
        readonly Lazy<GamesViewModel> _gvm;
        readonly IMediator _mediator;
        bool _areAdvancedStartupParamsVisible;
        GameSettingsDataModel _gameSettings;

        public GameSettingsOverlayViewModel(Lazy<GamesViewModel> gvm, IMediator mediator, IDialogManager dialogManager) {
            DisplayName = "Game Settings";
            _gvm = gvm;
            _mediator = mediator;

            this.SetCommand(x => x.ShowAdvancedStartupParamsCommand).Subscribe(ShowAdvancedStartupParams);
            this.SetCommand(x => x.GoBikGameStartupParameters).Subscribe(GoBikGameStartupParametersInfo);

            DiagnosticsMenu = new GameDiagnosticsMenu(dialogManager);

            this.WhenAnyObservable(x => x.GameSettings.Changed)
                .Where(x => x.PropertyName != "IsValid" && x.PropertyName != "Error")
                .Subscribe(x => SaveSettings(GameSettings));
        }

        public GameDiagnosticsMenu DiagnosticsMenu { get; private set; }
        public ReactiveCommand ShowAdvancedStartupParamsCommand { get; private set; }
        public ReactiveCommand GoBikGameStartupParameters { get; private set; }
        public bool AreAdvancedStartupParamsVisible
        {
            get { return _areAdvancedStartupParamsVisible; }
            set { SetProperty(ref _areAdvancedStartupParamsVisible, value); }
        }
        public GameSettingsDataModel GameSettings
        {
            get { return _gameSettings; }
            set { SetProperty(ref _gameSettings, value); }
        }

        void SaveSettings(GameSettingsDataModel x) {
            if (!x.IsValid)
                return;
            _mediator.Request(new SaveGameSettingsCommand {Settings = x});
        }

        [ReportUsage]
        void GoBikGameStartupParametersInfo(object x) {
            Tools.Generic.TryOpenUrl(BikiUrl);
        }

        [ReportUsage]
        void ShowAdvancedStartupParams(object x) {
            AreAdvancedStartupParamsVisible = !AreAdvancedStartupParamsVisible;
        }
    }
}