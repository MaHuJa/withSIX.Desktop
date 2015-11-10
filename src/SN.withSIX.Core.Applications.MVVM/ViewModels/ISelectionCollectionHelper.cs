// <copyright company="SIX Networks GmbH" file="ISelectionCollectionHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface ISelectionCollectionHelper<T> : INotifyPropertyChanged, IHaveReactiveItems<T>,
        IHaveSelectedItem<T> {}


    public static class SelectionCollectionHelperExtensions
    {
        public static ISelectionCollectionHelper<T> ToSelectionCollectionHelper<T>(this IEnumerable<T> input,
            T selectedItem = default(T)) {
            return new SelectionCollectionHelper<T>(input, selectedItem);
        }
    }

    public interface IIsEmpty
    {
        bool IsEmpty();
    }
}