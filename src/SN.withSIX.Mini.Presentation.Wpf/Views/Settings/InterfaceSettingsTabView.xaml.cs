// <copyright company="SIX Networks GmbH" file="InterfaceSettingsTabView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Applications.Views.Settings;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Settings
{
    /// <summary>
    ///     Interaction logic for InterfaceSettingsTabView.xaml
    /// </summary>
    public partial class InterfaceSettingsTabView : UserControl, IInterfaceSettingsTabView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IInterfaceSettingsTabViewModel),
                typeof (InterfaceSettingsTabView),
                new PropertyMetadata(null));

        public InterfaceSettingsTabView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.DataContext));

                d(this.Bind(ViewModel, vm => vm.Version, v => v.VersionText.Text));
                d(this.Bind(ViewModel, vm => vm.OptOutReporting, v => v.OptOut.IsChecked));
                d(this.Bind(ViewModel, vm => vm.ShowDesktopNotifications, v => v.ShowDesktopNotifications.IsChecked));
                d(this.Bind(ViewModel, vm => vm.StartWithWindows, v => v.StartWithWindows.IsChecked));
                if (Common.Flags.Verbose)
                    StartWithWindows.Visibility = Visibility.Collapsed;

                d(this.BindCommand(ViewModel, vm => vm.SaveLogs, v => v.SaveLogs));
                d(this.BindCommand(ViewModel, vm => vm.StartInDiagnosticsMode, v => v.DiagnosticsMode));
                d(this.BindCommand(ViewModel, vm => vm.ViewLicense, v => v.LicenseLink));
                d(this.BindCommand(ViewModel, vm => vm.ImportPwsSettings, v => v.ImportPwsSettings));
            });
        }

        public IInterfaceSettingsTabViewModel ViewModel
        {
            get { return (IInterfaceSettingsTabViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IInterfaceSettingsTabViewModel) value; }
        }
    }
}