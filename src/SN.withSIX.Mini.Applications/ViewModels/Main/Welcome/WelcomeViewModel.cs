// <copyright company="SIX Networks GmbH" file="WelcomeViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Welcome
{
    public class WelcomeViewModel : ViewModel, IWelcomeViewModel
    {
        public IReactiveCommand<object> Close { get; } = ReactiveCommand.Create();
    }

    public interface IWelcomeViewModel : IViewModel
    {
        IReactiveCommand<object> Close { get; }
    }
}