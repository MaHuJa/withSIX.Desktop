// <copyright company="SIX Networks GmbH" file="CollectionContentLibraryDataGrid.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library.CollectionLibraryItem
{
    /// <summary>
    ///     Interaction logic for CollectionContentLibraryDataGrid.xaml
    /// </summary>
    public partial class CollectionContentLibraryDataGrid : DataGridView
    {
        public CollectionContentLibraryDataGrid() {
            InitializeComponent();
            Setup(dg);
        }
    }
}