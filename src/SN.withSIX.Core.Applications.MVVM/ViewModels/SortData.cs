// <copyright company="SIX Networks GmbH" file="SortData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public class SortData : PropertyChangedBase
    {
        ListSortDirection _sortDirection;
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public ListSortDirection SortDirection
        {
            get { return _sortDirection; }
            set { SetProperty(ref _sortDirection, value); }
        }

        public SortDescription ToSortDescription() {
            return new SortDescription(Value, SortDirection);
        }
    }
}