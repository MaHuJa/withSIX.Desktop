// <copyright company="SIX Networks GmbH" file="InstalledView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Core.Presentation.Wpf.Extensions;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed;
using SN.withSIX.Mini.Applications.Views.Main.Games.Installed;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Games.Installed
{
    /// <summary>
    ///     Interaction logic for LocalView.xaml
    /// </summary>
    public partial class InstalledView : UserControl, IInstalledView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IInstalledViewModel), typeof (InstalledView),
                new PropertyMetadata(null));

        public InstalledView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                var hasItems = this.WhenAnyValue(x => x.ViewModel.EnabledItems.Count, Converters.VisibilityNormal);
                d(hasItems.BindTo(this, v => v.Clear.Visibility));
                d(hasItems.BindTo(this, v => v.PlaySelected.Visibility));
                d(hasItems.BindTo(this, v => v.UninstallSelected.Visibility));

                d(this.OneWayBind(ViewModel, x => x.LocalContent.Count, v => v.InstalledContent.Visibility,
                    Converters.VisibilityNormal));
                d(this.OneWayBind(ViewModel, x => x.LocalContent.Count, v => v.AddSomeContent.Visibility,
                    Converters.ReverseVisibility));
                d(this.OneWayBind(ViewModel, vm => vm.LocalContent, v => v.LocalItems.ItemsSource));
                d(this.BindCommand(ViewModel, vm => vm.UninstallSelected, v => v.UninstallSelected));
                d(this.BindCommand(ViewModel, vm => vm.PlaySelected, v => v.PlaySelected));
                d(this.BindCommand(ViewModel, vm => vm.Clear, v => v.Clear));
                d(this.BindCommand(ViewModel, vm => vm.AddContent, v => v.AddSomeContent));
            });
        }

        public IInstalledViewModel ViewModel
        {
            get { return (IInstalledViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IInstalledViewModel) value; }
        }
    }
}