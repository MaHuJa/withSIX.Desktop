// <copyright company="SIX Networks GmbH" file="IFlyoutOpener.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Applications.Services
{
    public interface IFlyoutOpener
    {
        void OpenFlyout<T>(T viewModel) where T : class, IFlyoutViewModel;
    }

    public interface IFlyoutScreen
    {
        IReactiveCommand ShowFlyout { get; }
    }
}