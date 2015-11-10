// <copyright company="SIX Networks GmbH" file="RoutableViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public interface IRoutableViewModel : ReactiveUI.IRoutableViewModel, ISomeViewModel {}


    public abstract class RoutableViewModel : SomeViewModel, IRoutableViewModel
    {
        protected RoutableViewModel(IScreen screen) {
            HostScreen = screen;
        }

        public override string DisplayName => UrlPathSegment;
        public abstract string UrlPathSegment { get; }
        public IScreen HostScreen { get; }
    }
}