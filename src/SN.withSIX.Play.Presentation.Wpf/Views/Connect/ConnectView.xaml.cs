// <copyright company="SIX Networks GmbH" file="ConnectView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.Views.Connect;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Connect
{
    [DoNotObfuscate]
    public partial class ConnectView : UserControl, IConnectView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (ConnectViewModel), typeof (ConnectView),
                new PropertyMetadata(null));

        public ConnectView() {
            InitializeComponent();
            var collectionViewSource = GetCvs();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.WhenAnyValue(x => x.ViewModel.SelectedChat, x => x.ViewModel.SelectedChat.ChatMessageEditor.Body,
                    (chat, body) => true)
                    // Need the throttle to always end up focussed, would be nice if we could find a better solution to this?
                    .Throttle(TimeSpan.FromMilliseconds(750))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => ChatControl.MessageEntry.Focus()));

                // FilterLIveShaping {"Model.DisplayName", "Model.Status", "Model.PlayingOn"}
                if (!Execute.InDesignMode) {
                    d(this.WhenAnyObservable(x => x.ViewModel.ContactFilter.FilterChanged)
                        .Skip(1)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => collectionViewSource.TryRefreshIfHasView()));
                }
            });
        }

        public ConnectViewModel ViewModel
        {
            get { return (ConnectViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ConnectViewModel; }
        }

        CollectionViewSource GetCvs() {
            return (CollectionViewSource) ContactListControl.FindResource("Contacts");
        }
    }
}