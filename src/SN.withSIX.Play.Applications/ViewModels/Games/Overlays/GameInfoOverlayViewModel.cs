// <copyright company="SIX Networks GmbH" file="GameInfoOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class GameInfoOverlayViewModel : OverlayViewModelBase, ISingleton
    {
        readonly Lazy<GamesViewModel> _gvm;

        public GameInfoOverlayViewModel(Lazy<GamesViewModel> gvm) {
            DisplayName = "Game Info";
            _gvm = gvm;
        }

        public GamesViewModel GVM
        {
            get { return _gvm.Value; }
        }

        [DoNotObfuscate]
        public void Purchase() {
            GVM.PurchaseGame(GVM.SelectedItem);
        }
    }
}