// <copyright company="SIX Networks GmbH" file="CollectionItemView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    /// <summary>
    ///     Interaction logic for CollectionItemView.xaml
    /// </summary>
    public partial class CollectionItemView : UserControl, IViewFor<Collection>, IViewFor<CustomCollection>,
        IViewFor<SubscribedCollection>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (Collection), typeof (CollectionItemView),
                new PropertyMetadata(null));

        public CollectionItemView() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public Collection ViewModel
        {
            get { return (Collection) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (Collection) value; }
        }
        CustomCollection IViewFor<CustomCollection>.ViewModel
        {
            get { return (CustomCollection) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        SubscribedCollection IViewFor<SubscribedCollection>.ViewModel
        {
            get { return (SubscribedCollection) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}