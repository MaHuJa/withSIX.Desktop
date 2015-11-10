// <copyright company="SIX Networks GmbH" file="DownloadSettingsTabView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Applications.Views.Settings;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Settings
{
    /// <summary>
    ///     Interaction logic for DownloadSettingsTabView.xaml
    /// </summary>
    public partial class DownloadSettingsTabView : UserControl, IDownloadSettingsTabView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IDownloadSettingsTabViewModel),
                typeof (DownloadSettingsTabView),
                new PropertyMetadata(null));

        public DownloadSettingsTabView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public IDownloadSettingsTabViewModel ViewModel
        {
            get { return (IDownloadSettingsTabViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IDownloadSettingsTabViewModel) value; }
        }
    }
}