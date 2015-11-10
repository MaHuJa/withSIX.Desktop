// <copyright company="SIX Networks GmbH" file="MissionFolderItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for MissionFolderItemView.xaml
    /// </summary>
    public partial class MissionFolderItemView : UserControl, IViewFor<MissionFolder>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (MissionFolder), typeof (MissionFolderItemView),
                new PropertyMetadata(null));

        public MissionFolderItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public MissionFolder ViewModel
        {
            get { return (MissionFolder) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MissionFolder) value; }
        }
    }
}