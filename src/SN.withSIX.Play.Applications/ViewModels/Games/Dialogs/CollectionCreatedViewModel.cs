// <copyright company="SIX Networks GmbH" file="CollectionCreatedViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Dialogs
{
    public interface ICollectionCreatedViewModel {}

    [DoNotObfuscate]
    public class CollectionCreatedViewModel : DialogBase, ICollectionCreatedViewModel
    {
        readonly Lazy<ModsViewModel> _mods;
        readonly ExportFactory<PickContactViewModel> _pickContactFactory;

        public CollectionCreatedViewModel(ExportFactory<PickContactViewModel> pickContactFactory,
            Lazy<ModsViewModel> mods) {
            _pickContactFactory = pickContactFactory;
            _mods = mods;

            this.SetCommand(x => x.OkCommand).Subscribe(() => TryClose());
            ReactiveUI.ReactiveCommand.CreateAsyncTask(x => ShareWithFriends())
                .SetNewCommand(this, x => x.ShareCommand)
                .Subscribe();
            DisplayName = "Share the links to the published collection";
        }

        public Uri PwsUrl { get; private set; }
        public Uri OnlineUrl { get; private set; }
        public CustomCollectionLibraryItemViewModel Collection { get; private set; }
        public ReactiveCommand OkCommand { get; private set; }
        public ReactiveCommand<Unit> ShareCommand { get; private set; }

        async Task ShareWithFriends() {
            using (var vm = _pickContactFactory.CreateExport()) {
                await vm.Value.Load(Collection.Model);
                vm.Value.SetCurrent(null);
                _mods.Value.ShowOverlay(vm.Value);
            }
        }

        public void SetCollection(CustomCollectionLibraryItemViewModel collection) {
            Contract.Requires<ArgumentNullException>(collection != null);

            Collection = collection;
            OnlineUrl = collection.Model.ProfileUrl();
            PwsUrl = collection.Model.GetPwsUri();
        }
    }
}