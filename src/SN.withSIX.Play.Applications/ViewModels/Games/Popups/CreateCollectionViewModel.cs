// <copyright company="SIX Networks GmbH" file="CreateCollectionViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Popups
{
    public interface ICreateCollectionViewModel
    {
        string CollectionName { get; set; }
        ReactiveCommand<Unit> CreateCollection { get; }
    }

    public class CreateCollectionViewModel : PopupBase, ICreateCollectionViewModel
    {
        readonly IContentManager _contentManager;
        string _collectionName;

        public CreateCollectionViewModel(IContentManager contentManager) {
            _contentManager = contentManager;

            CollectionName = _contentManager.GetSuggestedCollectionName();

            ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.CollectionName)
                .Select(x => !string.IsNullOrWhiteSpace(x)), CreateCollectionInternal)
                .SetNewCommand(this, x => x.CreateCollection)
                .Subscribe(x => TryClose());
        }

        public string CollectionName
        {
            get { return _collectionName; }
            set { SetProperty(ref _collectionName, value); }
        }
        public ReactiveCommand<Unit> CreateCollection { get; private set; }

        async Task CreateCollectionInternal(object x) {
            var collection = _contentManager.CreateAndSelectCustomModSet();
            collection.Name = CollectionName;
        }
    }
}