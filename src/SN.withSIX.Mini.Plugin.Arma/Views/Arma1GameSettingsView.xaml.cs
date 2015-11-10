// <copyright company="SIX Networks GmbH" file="Arma1GameSettingsView.xaml.cs">
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
    public partial class Arma1GameSettingsView : UserControl, IArma1GameSettingsView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IArma1GameSettingsViewModel),
                typeof (Arma1GameSettingsView),
                new PropertyMetadata(null));

        public Arma1GameSettingsView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.Bind(ViewModel, vm => vm.GameDirectory, v => v.GameDirectory.Text));
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

        public IArma1GameSettingsViewModel ViewModel
        {
            get { return (IArma1GameSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IArma1GameSettingsViewModel) value; }
        }
    }

    public interface IArma1GameSettingsView : IViewFor<IArma1GameSettingsViewModel> {}
}