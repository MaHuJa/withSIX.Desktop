// <copyright company="SIX Networks GmbH" file="DefaultLibraryContentControl.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    public interface IDefaultLibraryContentControl : IViewFor<IHaveSelectedItemsView> {}

    /// <summary>
    ///     Interaction logic for DefaultLibraryContentControl.xaml
    /// </summary>
    public partial class DefaultLibraryContentControl : UserControl, IDefaultLibraryContentControl
    {
        public static readonly DependencyProperty ListBoxStyleProperty = DependencyProperty.Register("ListBoxStyle",
            typeof (Style
                ), typeof (DefaultLibraryContentControl), new PropertyMetadata(default(Style
                    )));
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IHaveSelectedItemsView),
                typeof (DefaultLibraryContentControl),
                new PropertyMetadata(null));

        public DefaultLibraryContentControl() {
            InitializeComponent();
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.ItemsView, v => v.Lb.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedItem, v => v.Lb.SelectedItem));
                d(this.WhenAnyValue(x => x.Lb.SelectedItems)
                    .Cast<ObservableCollection<object>>()
                    .BindTo(this, x => x.ViewModel.SelectedItemsInternal));
            });
        }

        public Style
            ListBoxStyle
        {
            get
            {
                return (Style
                    ) GetValue(ListBoxStyleProperty);
            }
            set { SetValue(ListBoxStyleProperty, value); }
        }
        public IHaveSelectedItemsView ViewModel
        {
            get { return (IHaveSelectedItemsView) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IHaveSelectedItemsView) value; }
        }
    }
}