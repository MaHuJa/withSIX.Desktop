// <copyright company="SIX Networks GmbH" file="DataGridView.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Library
{
    public interface IDataGridView : IViewFor<IHaveSelectedItems> {}

    public abstract class DataGridView : UserControl, IDataGridView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IHaveSelectedItems), typeof (DataGridView),
                new PropertyMetadata(null));

        protected DataGridView() {
            this.WhenActivated(d => { d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext)); });
        }

        public IHaveSelectedItems ViewModel
        {
            get { return (IHaveSelectedItems) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IHaveSelectedItems) value; }
        }
        // tODO: Better to just pass along SelectedItems as DP?
        protected void Setup(DataGrid dg) {
            this.WhenActivated(d => {
                d(dg.WhenAnyValue(x => x.SelectedItems)
                    .Cast<ObservableCollection<object>>()
                    .BindTo(this, x => x.ViewModel.SelectedItemsInternal));
            });
        }
    }
}