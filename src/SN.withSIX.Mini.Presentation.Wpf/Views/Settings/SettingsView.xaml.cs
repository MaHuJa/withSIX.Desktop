// <copyright company="SIX Networks GmbH" file="SettingsView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Applications.Views.Settings;
using SN.withSIX.Mini.Presentation.Wpf.Extensions;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Settings
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsView : MetroWindow, ISettingsView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ISettingsViewModel), typeof (SettingsView),
                new PropertyMetadata(null));

        public SettingsView() {
            InitializeComponent();

            Closed += OnClosedHandler;

            this.WhenActivated(d => {
                this.SetupScreen<ISettingsViewModel>(d);

                d(this.OneWayBind(ViewModel, vm => vm.Settings.Items, v => v.Settings.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.Settings.SelectedItem, v => v.Settings.SelectedItem));
                d(this.BindCommand(ViewModel, vm => vm.Help, v => v.HelpButton));
                d(this.BindCommand(ViewModel, vm => vm.Ok, v => v.OkButton));
                // This approach does not call the Cancel Command on the ViewModel if the Window close button was pressed
                // This can however be accomplished by leveraging: http://stackoverflow.com/questions/20378154/mahapps-metro-capture-close-window-event
                d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton));
                d(this.WhenAnyObservable(v => v.ViewModel.Ok).Select(x => (object) null)
                    .Merge(this.WhenAnyObservable(v => v.ViewModel.Cancel))
                    .Subscribe(x => Close()));
            });
        }

        public ISettingsViewModel ViewModel
        {
            get { return (ISettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ISettingsViewModel) value; }
        }

        void OnClosedHandler(object sender, EventArgs args) {
            ViewModel.IsOpen = false;
        }
    }
}