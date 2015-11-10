// <copyright company="SIX Networks GmbH" file="ServerLibraryDataGridView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library.ServerLibraryItem
{
    /// <summary>
    ///     Interaction logic for ServerLibraryDataGridView.xaml
    /// </summary>
    public partial class ServerLibraryDataGridView : DataGridView
    {
        public ServerLibraryDataGridView() {
            InitializeComponent();
            Setup(dg);
        }
    }
}