// <copyright company="SIX Networks GmbH" file="DesignTimeModsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class DesignTimeModsViewModel : ModsViewModel, IDesignTimeViewModel
    {
        public DesignTimeModsViewModel() {
            LibraryVM = new DesignTimeModLibraryViewModel();
        }
    }
}