// <copyright company="SIX Networks GmbH" file="ModItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for ModItemView.xaml
    /// </summary>
    public partial class ModItemView : UserControl, IViewFor<Mod>, IViewFor<LocalMod>, IViewFor<CustomRepoMod>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (Mod), typeof (ModItemView),
                new PropertyMetadata(null));

        public ModItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        CustomRepoMod IViewFor<CustomRepoMod>.ViewModel
        {
            get { return (CustomRepoMod) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        LocalMod IViewFor<LocalMod>.ViewModel
        {
            get { return (LocalMod) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public Mod ViewModel
        {
            get { return (Mod) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (Mod) value; }
        }
    }
}