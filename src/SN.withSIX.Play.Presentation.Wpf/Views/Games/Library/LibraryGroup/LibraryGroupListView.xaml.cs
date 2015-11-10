// <copyright company="SIX Networks GmbH" file="LibraryGroupListView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games.Library.LibraryGroup;
using SN.withSIX.Play.Applications.Views.Games.Library.LibraryGroup;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library.LibraryGroup
{
    /// <summary>
    ///     Interaction logic for LibraryGroupListView.xaml
    /// </summary>
    [ViewContract(Contract = ViewTypeString.List)]
    public partial class LibraryGroupListView : UserControl, ILibraryGroupListView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (LibraryGroupViewModel),
                typeof (LibraryGroupListView),
                new PropertyMetadata(null));

        public LibraryGroupListView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public LibraryGroupViewModel ViewModel
        {
            get { return (LibraryGroupViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as LibraryGroupViewModel; }
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