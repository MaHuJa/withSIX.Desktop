// <copyright company="SIX Networks GmbH" file="Arma2COGameSettingsView.xaml.cs">
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
    ///     Interaction logic for Arma2COGameSettingsView.xaml
    /// </summary>
    public partial class Arma2COGameSettingsView : UserControl, IArma2COGameSettingsView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IArma2COGameSettingsViewModel),
                typeof (Arma2COGameSettingsView),
                new PropertyMetadata(null));

        public Arma2COGameSettingsView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.Bind(ViewModel, vm => vm.GameDirectory, v => v.GameDirectory.Text));
                d(this.Bind(ViewModel, vm => vm.Arma2GameDirectory, v => v.Arma2GameDirectory.Text));
                d(this.Bind(ViewModel, vm => vm.PackageDirectory, v => v.ModDirectory.Text));
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

        public IArma2COGameSettingsViewModel ViewModel
        {
            get { return (IArma2COGameSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IArma2COGameSettingsViewModel) value; }
        }
    }

    public interface IArma2COGameSettingsView : IViewFor<IArma2COGameSettingsViewModel> {}
}