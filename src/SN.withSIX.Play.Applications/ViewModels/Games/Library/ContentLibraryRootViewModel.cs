// <copyright company="SIX Networks GmbH" file="ContentLibraryRootViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public abstract class ContentLibraryRootViewModel :
        LibraryRootViewModel<ContentLibraryItemViewModel, IContent, SearchContentLibraryItemViewModel>
    {
        readonly ModuleViewModelBase _module;

        protected ContentLibraryRootViewModel(ModuleViewModelBase module) {
            _module = module;
        }
    }
}