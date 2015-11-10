// <copyright company="SIX Networks GmbH" file="CollectionCreatedViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Dialogs
{
    public interface ICollectionCreatedViewModel {}

    [DoNotObfuscate]
    public class CollectionCreatedViewModel : DialogBase, ICollectionCreatedViewModel
    {
        readonly Lazy<ModsViewModel> _mods;

        public CollectionCreatedViewModel(Lazy<ModsViewModel> mods) {
            _mods = mods;

            this.SetCommand(x => x.OkCommand).Subscribe(() => TryClose());
            DisplayName = "Share the links to the published collection";
        }

        public Uri PwsUrl { get; private set; }
        public Uri OnlineUrl { get; private set; }
        public CustomCollectionLibraryItemViewModel Collection { get; private set; }
        public ReactiveCommand OkCommand { get; private set; }
        public ReactiveCommand<Unit> ShareCommand { get; private set; }

        public void SetCollection(CustomCollectionLibraryItemViewModel collection) {
            Contract.Requires<ArgumentNullException>(collection != null);

            Collection = collection;
            OnlineUrl = collection.Model.ProfileUrl();
            PwsUrl = collection.Model.GetPwsUri();
        }
    }
}