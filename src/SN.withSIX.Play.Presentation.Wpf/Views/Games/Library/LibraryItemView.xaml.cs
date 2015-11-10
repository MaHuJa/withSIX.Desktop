// <copyright company="SIX Networks GmbH" file="LibraryItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for LibraryItemView.xaml
    /// </summary>
    public partial class LibraryItemView : UserControl, IViewFor<LibraryItemViewModel>,
        IViewFor<ContentLibraryItemViewModel>,
        IViewFor<LocalModsLibraryItemViewModel>,
        IViewFor<MissionLibrarySetup.LocalMissionLibraryItemViewModel>, IViewFor<NetworkLibraryItemViewModel>,
        IViewFor<ServerLibrarySetup.NetworkLibraryItemViewModel>,
        IViewFor<MissionLibrarySetup.NetworkLibraryItemViewModel>,
        IViewFor<BrowseContentLibraryItemViewModel<BuiltInContentContainer>>,
        IViewFor<BrowseContentLibraryItemViewModel<SixRepo>>,
        IViewFor<MissionContentLibraryItemViewModel<BuiltInContentContainer>>,
        IViewFor<ServerLibraryItemViewModel<BuiltInServerContainer>>
        // INSANE!!
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (LibraryItemViewModel), typeof (LibraryItemView),
                new PropertyMetadata(null));

        public LibraryItemView() {
            InitializeComponent();
        }

        BrowseContentLibraryItemViewModel<BuiltInContentContainer>
            IViewFor<BrowseContentLibraryItemViewModel<BuiltInContentContainer>>.ViewModel
        {
            get { return (BrowseContentLibraryItemViewModel<BuiltInContentContainer>) ViewModel; }
            set { ViewModel = value; }
        }
        BrowseContentLibraryItemViewModel<SixRepo> IViewFor<BrowseContentLibraryItemViewModel<SixRepo>>.ViewModel
        {
            get { return (BrowseContentLibraryItemViewModel<SixRepo>) ViewModel; }
            set { ViewModel = value; }
        }
        ContentLibraryItemViewModel IViewFor<ContentLibraryItemViewModel>.ViewModel
        {
            get { return (ContentLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (LibraryItemViewModel) value; }
        }
        public LibraryItemViewModel ViewModel
        {
            get { return (LibraryItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        MissionLibrarySetup.LocalMissionLibraryItemViewModel
            IViewFor<MissionLibrarySetup.LocalMissionLibraryItemViewModel>.ViewModel
        {
            get { return (MissionLibrarySetup.LocalMissionLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        LocalModsLibraryItemViewModel IViewFor<LocalModsLibraryItemViewModel>.ViewModel
        {
            get { return (LocalModsLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        MissionContentLibraryItemViewModel<BuiltInContentContainer>
            IViewFor<MissionContentLibraryItemViewModel<BuiltInContentContainer>>.ViewModel
        {
            get { return (MissionContentLibraryItemViewModel<BuiltInContentContainer>) ViewModel; }
            set { ViewModel = value; }
        }
        NetworkLibraryItemViewModel IViewFor<NetworkLibraryItemViewModel>.ViewModel
        {
            get { return (NetworkLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        ServerLibrarySetup.NetworkLibraryItemViewModel IViewFor<ServerLibrarySetup.NetworkLibraryItemViewModel>.
            ViewModel
        {
            get { return (ServerLibrarySetup.NetworkLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        MissionLibrarySetup.NetworkLibraryItemViewModel IViewFor<MissionLibrarySetup.NetworkLibraryItemViewModel>.
            ViewModel
        {
            get { return (MissionLibrarySetup.NetworkLibraryItemViewModel) ViewModel; }
            set { ViewModel = value; }
        }
        ServerLibraryItemViewModel<BuiltInServerContainer> IViewFor<ServerLibraryItemViewModel<BuiltInServerContainer>>.
            ViewModel
        {
            get { return (ServerLibraryItemViewModel<BuiltInServerContainer>) ViewModel; }
            set { ViewModel = value; }
        }
    }
}