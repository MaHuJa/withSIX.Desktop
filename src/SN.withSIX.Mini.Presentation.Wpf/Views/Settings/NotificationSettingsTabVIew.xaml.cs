// <copyright company="SIX Networks GmbH" file="NotificationSettingsTabView.xaml.cs">
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
    ///     Interaction logic for NotificationSettingsTabVIew.xaml
    /// </summary>
    public partial class NotificationSettingsTabView : UserControl, INotificationSettingsTabView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (INotificationSettingsTabViewModel),
                typeof (NotificationSettingsTabView),
                new PropertyMetadata(null));

        public NotificationSettingsTabView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public INotificationSettingsTabViewModel ViewModel
        {
            get { return (INotificationSettingsTabViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (INotificationSettingsTabViewModel) value; }
        }
    }
}