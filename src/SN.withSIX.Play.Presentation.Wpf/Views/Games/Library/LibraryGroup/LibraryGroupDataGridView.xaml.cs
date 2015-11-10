// <copyright company="SIX Networks GmbH" file="LibraryGroupDataGridView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Applications.Views.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library.LibraryGroup
{
    /// <summary>
    ///     Interaction logic for LibraryGroupDataGrid.xaml
    /// </summary>
    [ViewContract(Contract = ViewTypeString.Grid)]
    public partial class LibraryGroupDataGridView : DataGridView, ILibraryGroupDataGridView
    {
        public LibraryGroupDataGridView() {
            InitializeComponent();
            Setup(dg);
        }

        public new LibraryGroupViewModel ViewModel
        {
            get { return (LibraryGroupViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }
        ModLibraryGroupViewModel IViewFor<ModLibraryGroupViewModel>.ViewModel
        {
            get { return (ModLibraryGroupViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        MissionLibraryGroupViewModel IViewFor<MissionLibraryGroupViewModel>.ViewModel
        {
            get { return (MissionLibraryGroupViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        ServerLibraryGroupViewModel IViewFor<ServerLibraryGroupViewModel>.ViewModel
        {
            get { return (ServerLibraryGroupViewModel) ViewModel; }
            set { ViewModel = value; }
        }
    }
}