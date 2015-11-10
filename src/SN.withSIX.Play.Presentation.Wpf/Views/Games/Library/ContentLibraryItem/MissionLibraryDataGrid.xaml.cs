// <copyright company="SIX Networks GmbH" file="MissionLibraryDataGrid.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library.ContentLibraryItem
{
    /// <summary>
    ///     Interaction logic for MissionLibraryDataGrid.xaml
    /// </summary>
    public partial class MissionLibraryDataGrid : DataGridView
    {
        public MissionLibraryDataGrid() {
            InitializeComponent();
            Setup(dg);
        }
    }
}