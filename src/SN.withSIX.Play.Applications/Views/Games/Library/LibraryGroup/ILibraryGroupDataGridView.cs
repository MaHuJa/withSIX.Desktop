// <copyright company="SIX Networks GmbH" file="ILibraryGroupDataGridView.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;

namespace SN.withSIX.Play.Applications.Views.Games.Library.LibraryGroup
{
    public interface ILibraryGroupDataGridView : IViewFor<LibraryGroupViewModel>,
        IModLibraryGroupView,
        IViewFor<MissionLibraryGroupViewModel>,
        IViewFor<ServerLibraryGroupViewModel> {}
}