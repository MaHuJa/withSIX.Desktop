// <copyright company="SIX Networks GmbH" file="WelcomeView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels.Main.Welcome;
using SN.withSIX.Mini.Applications.Views.Main.Welcome;

namespace SN.withSIX.Mini.Presentation.Wpf.Views.Main.Welcome
{
    /// <summary>
    ///     Interaction logic for WelcomeView.xaml
    /// </summary>
    public partial class WelcomeView : UserControl, IWelcomeView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IWelcomeViewModel), typeof (WelcomeView),
                new PropertyMetadata(null));

        public WelcomeView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.BindCommand(ViewModel, vm => vm.Close, v => v.Close));
            });
        }

        public IWelcomeViewModel ViewModel
        {
            get { return (IWelcomeViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IWelcomeViewModel) value; }
        }
    }
}