// <copyright company="SIX Networks GmbH" file="TabViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public abstract class TabViewModel : ViewModel, ITabViewModel
    {
        bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public abstract string DisplayName { get; }
        public abstract string Icon { get; }
    }

    public interface ITabViewModel : IViewModel, IHaveDisplayName, IHaveIcon, ISelectable {}
}