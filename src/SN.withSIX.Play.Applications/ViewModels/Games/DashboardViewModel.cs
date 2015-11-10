// <copyright company="SIX Networks GmbH" file="DashboardViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public interface IDashboardViewModel : IViewModel, IRoutableViewModel
    {
        ReactiveCommand<object> GoGames { get; }
        ReactiveCommand<object> GoDiscovery { get; }
    }

    public class DashboardViewModel : ReactiveObject, IDashboardViewModel
    {
        public DashboardViewModel(IPlayShellViewModel screen) {
            HostScreen = screen;

            ReactiveCommand.CreateAsyncTask(x => {
                screen.Content.GoGames();
                return screen.Router.Navigate.ExecuteAsyncTask(screen.Content);
            }).SetNewCommand(this, x => x.GoGames);

            ReactiveCommand.CreateAsyncTask(x => {
                screen.Content.GoHome();
                return screen.Router.Navigate.ExecuteAsyncTask(screen.Content);
            }).SetNewCommand(this, x => x.GoDiscovery);
        }

        public string UrlPathSegment
        {
            get { return "dashboard"; }
        }
        public IScreen HostScreen { get; }
        public ReactiveCommand<object> GoGames { get; private set; }
        public ReactiveCommand<object> GoDiscovery { get; private set; }
    }
}