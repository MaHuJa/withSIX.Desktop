// <copyright company="SIX Networks GmbH" file="AboutViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Mini.Applications.Usecases;

namespace SN.withSIX.Mini.Applications.ViewModels.About
{
    public class AboutViewModel : ScreenViewModel, IAboutViewModel
    {
        public override string DisplayName { get; } = "About";
        public string Version { get; set; }
        public ICommand ViewLicense { get; } =
            ReactiveCommand.CreateAsyncTask(x => RequestAsync(new OpenWebLink(ViewType.License)));
    }

    public interface IAboutViewModel : IScreenViewModel
    {
        string Version { get; }
        ICommand ViewLicense { get; }
    }
}