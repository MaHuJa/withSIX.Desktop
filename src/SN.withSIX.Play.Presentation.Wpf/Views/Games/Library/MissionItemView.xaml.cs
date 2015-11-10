// <copyright company="SIX Networks GmbH" file="MissionItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for MissionItemView.xaml
    /// </summary>
    public partial class MissionItemView : UserControl, IViewFor<Mission>, IViewFor<LocalMission>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (Mission), typeof (MissionItemView),
                new PropertyMetadata(null));

        public MissionItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        LocalMission IViewFor<LocalMission>.ViewModel
        {
            get { return (LocalMission) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public Mission ViewModel
        {
            get { return (Mission) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (Mission) value; }
        }
    }
}