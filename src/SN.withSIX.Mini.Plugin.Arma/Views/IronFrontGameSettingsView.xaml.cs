// <copyright company="SIX Networks GmbH" file="IronFrontGameSettingsView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Plugin.Arma.ViewModels;

namespace SN.withSIX.Mini.Plugin.Arma.Views
{
    /// <summary>
    ///     Interaction logic for IronFrontGameSettingsView.xaml
    /// </summary>
    public partial class IronFrontGameSettingsView : UserControl, IIronFrontGameSettingsView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IIronFrontGameSettingsViewModel),
                typeof (IronFrontGameSettingsView),
                new PropertyMetadata(null));

        public IronFrontGameSettingsView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.Bind(ViewModel, vm => vm.GameDirectory, v => v.GameDirectory.Text));
                //d(this.Bind(ViewModel, vm => vm.PackageDirectory, v => v.ModDirectory.Text));
                d(this.Bind(ViewModel, vm => vm.RepoDirectory, v => v.SynqDirectory.Text));
                d(this.OneWayBind(ViewModel, vm => vm.StartupParameters, v => v.StartupParameters.SelectedObject));
                d(this.OneWayBind(ViewModel, vm => vm.ShowStartupParameters, v => v.StartupParametersGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.ShowStartupParameters, v => v.SettingsGrid.Visibility,
                    Converters.ReverseVisibility));
                d(this.Bind(ViewModel, vm => vm.StartupParameters.StartupLine, v => v.StartupParametersText.Text));
                d(this.BindCommand(ViewModel, vm => vm.ToggleStartupParameters, v => v.ShowAdvancedEditor));
                d(this.BindCommand(ViewModel, vm => vm.ToggleStartupParameters, v => v.HideAdvancedEditor));
            });
        }

        public IIronFrontGameSettingsViewModel ViewModel
        {
            get { return (IIronFrontGameSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IIronFrontGameSettingsViewModel) value; }
        }
    }

    public interface IIronFrontGameSettingsView : IViewFor<IIronFrontGameSettingsViewModel> {}
}