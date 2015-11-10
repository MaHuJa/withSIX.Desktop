// <copyright company="SIX Networks GmbH" file="ModVersionOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class ModVersionOverlayViewModel : ModInfoOverlayViewModel
    {
        public ModVersionOverlayViewModel(Lazy<ModsViewModel> mvm) : base(mvm) {}
    }
}