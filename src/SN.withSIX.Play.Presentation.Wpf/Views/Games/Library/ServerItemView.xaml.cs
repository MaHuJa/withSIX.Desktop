// <copyright company="SIX Networks GmbH" file="ServerItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for ServerItemView.xaml
    /// </summary>
    public partial class ServerItemView : UserControl, IViewFor<Server>, IViewFor<ArmaServer>, IViewFor<Arma2FreeServer>,
        IViewFor<DayzServer>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (Server), typeof (ServerItemView),
                new PropertyMetadata(null));

        public ServerItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        Arma2FreeServer IViewFor<Arma2FreeServer>.ViewModel
        {
            get { return (Arma2FreeServer) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        ArmaServer IViewFor<ArmaServer>.ViewModel
        {
            get { return (ArmaServer) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        DayzServer IViewFor<DayzServer>.ViewModel
        {
            get { return (DayzServer) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public Server ViewModel
        {
            get { return (Server) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (Server) value; }
        }
    }
}