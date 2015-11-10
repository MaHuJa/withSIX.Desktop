// <copyright company="SIX Networks GmbH" file="ToggleableModItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for ToggleableModItemView.xaml
    /// </summary>
    public partial class ToggleableModItemView : UserControl, IViewFor<ToggleableModProxy>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ToggleableModProxy), typeof (ToggleableModItemView),
                new PropertyMetadata(null));

        public ToggleableModItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public ToggleableModProxy ViewModel
        {
            get { return (ToggleableModProxy) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ToggleableModProxy) value; }
        }
    }
}