// <copyright company="SIX Networks GmbH" file="ConnectPopoutView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.Views.Connect;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Connect
{
    /// <summary>
    ///     Interaction logic for ConnectPopout.xaml
    /// </summary>
    public partial class ConnectPopoutView : MetroWindow, IConnectPopoutView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ConnectPopoutViewModel), typeof (ConnectPopoutView),
                new PropertyMetadata(null));

        public ConnectPopoutView() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.WhenAnyValue(x => x.ViewModel.Connect.SelectedChat,
                    x => x.ViewModel.Connect.SelectedChat.ChatMessageEditor.Body,
                    (chat, body) => true)
                    // Need the throttle to always end up focussed, would be nice if we could find a better solution to this?
                    .Throttle(TimeSpan.FromMilliseconds(750))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => Chat.MessageEntry.Focus()));
            });
        }

        public ConnectPopoutViewModel ViewModel
        {
            get { return (ConnectPopoutViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ConnectPopoutViewModel; }
        }
    }
}