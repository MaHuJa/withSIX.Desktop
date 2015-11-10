// <copyright company="SIX Networks GmbH" file="CustomRepoAvailabilityWarningViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Dialogs
{
    public interface ICustomRepoAvailabilityWarningViewModel : IDialog {} // IMetroDialog

    [DoNotObfuscate]
    public class CustomRepoAvailabilityWarningViewModel : DialogBase, ICustomRepoAvailabilityWarningViewModel
    {
        readonly CustomCollection _customCollection;
        readonly SixRepo _customRepo;
        //ExportFactory<PickContactViewModel> pickContactFactory, Lazy<ModsViewModel> mods
        public CustomRepoAvailabilityWarningViewModel(CustomCollection customCollection) {
            DisplayName = "Third party content";
            _customRepo = customCollection.CustomRepo;
            _customCollection = customCollection;

            this.SetCommand(x => x.OkCommand).Subscribe(() => {
                HandleDontAskAgain();
                TryClose(true);
            });
        }

        public bool RememberedState { set; get; }
        public ReactiveCommand OkCommand { get; private set; }
        public string Author
        {
            get { return _customRepo.Name; }
        }

        void HandleDontAskAgain() {
            if (!RememberedState)
                return;
            _customCollection.SetRemember();
        }
    }
}