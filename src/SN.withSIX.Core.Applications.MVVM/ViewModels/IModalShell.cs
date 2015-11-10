// <copyright company="SIX Networks GmbH" file="IModalShell.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface IModalShell : INotifyPropertyChanged
    {
        IModalScreen ModalActiveItem { get; set; }
        bool ModalItemShowing { get; }
        void CancelModalView();
        void HideModalView();
        void ShowModalView(IModalScreen viewModel);
    }
}