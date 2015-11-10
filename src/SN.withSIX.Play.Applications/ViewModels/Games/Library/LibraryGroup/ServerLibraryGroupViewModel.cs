// <copyright company="SIX Networks GmbH" file="ServerLibraryGroupViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup
{
    public class ServerLibraryGroupViewModel : LibraryGroupViewModel<ServerLibraryViewModel>
    {
        public ServerLibraryGroupViewModel(ServerLibraryViewModel library, string header, string addHeader = null,
            string icon = null) : base(library, header, addHeader, icon) {}
    }
}