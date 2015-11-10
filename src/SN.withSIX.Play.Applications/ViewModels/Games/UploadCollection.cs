// <copyright company="SIX Networks GmbH" file="UploadCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.UseCases;
using SN.withSIX.Play.Applications.UseCases.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Dialogs;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class UploadCollection
    {
        readonly Func<CollectionCreatedViewModel> _collectionCreatedFactory;
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;

        public UploadCollection(IDialogManager dialogManager, IMediator mediator,
            Func<CollectionCreatedViewModel> collectionCreatedFactory) {
            _dialogManager = dialogManager;
            _mediator = mediator;
            _collectionCreatedFactory = collectionCreatedFactory;
        }

        public async Task<bool> Upload(CustomCollectionLibraryItemViewModel collection) {
            var scope = CollectionScope.Private;
            bool? result = null;
            if (!await WrapAsync(collection, async () => {
                var collectionVisibilityViewModel =
                    _mediator.Request(new ShowCollectionVisibilityQuery(collection.Model.Id));
                result = (await _dialogManager.ShowDialogAsync(collectionVisibilityViewModel)).GetValueOrDefault();
                scope = collectionVisibilityViewModel.Visibility;
            }))
                return false;

            if (!result.GetValueOrDefault())
                return false;

            await Publish(collection, scope).ConfigureAwait(false);

            if (scope != CollectionScope.Private)
                await ShowCollectionCreatedDialog(collection);

            return true;
        }

        public Task ShowCollectionCreatedDialog(CustomCollectionLibraryItemViewModel collection) {
            var collectionCreatedViewModel = _collectionCreatedFactory();
            collectionCreatedViewModel.SetCollection(collection);
            return _dialogManager.ShowDialogAsync(collectionCreatedViewModel);
        }

        public async Task<bool> Publish(CustomCollectionLibraryItemViewModel collection, CollectionScope scope) {
            if (!await DealWithCustomRepo(collection.Model))
                return false;

            return
                await
                    WrapAsync(collection,
                        () =>
                            _mediator.RequestAsyncWrapped(new PublishCollectionCommand(collection.Model.Id, scope,
                                collection.Model.ForkedCollectionId)))
                        .ConfigureAwait(false);
        }

        public async Task<bool> Sync(CustomCollectionLibraryItemViewModel collection) {
            if (!await DealWithCustomRepo(collection.Model))
                return false;
            return await
                WrapAsync(collection,
                    () => _mediator.RequestAsyncWrapped(new PublishNewCollectionVersionCommand(collection.Model.Id)))
                    .ConfigureAwait(false);
        }

        async Task<bool> DealWithCustomRepo(CustomCollection customCollection) {
            if (!customCollection.HasCustomRepo() ||
                (!customCollection.CustomRepoUrl.EndsWith(".yml") ||
                 customCollection.CustomRepoUrl.EndsWith("config.yml")))
                return true;
            var sixMessageBoxResult = await CustomRepoDialog(customCollection);
            return sixMessageBoxResult.IsYes();
        }

        Task<SixMessageBoxResult> CustomRepoDialog(CustomCollection customCollection) {
            return _dialogManager.MessageBoxAsync(
                new MessageBoxDialogParams(
                    @"Warning !!!

You are about to publish a collection that is linked to the following Custom Repository:

" + customCollection.CustomRepo.Name + "(" + customCollection.CustomRepoUrl + ")" + @"

Dependent on the publish state, the custom repository will be available publicly.

Are you authorized to publish the custom repository?",
                    "Are you authorized to publish this custom repository?", SixMessageBoxButton.YesNo));
        }

        async Task<bool> WrapAsync(CustomCollectionLibraryItemViewModel collection, Func<Task> task) {
            try {
                await task().ConfigureAwait(false);
                return true;
            } catch (CollectionEmptyException) {
                AddSomeItems();
            } catch (CollectionNameMissingException) {
                EditName(collection);
            } catch (CollectionDescriptionMissingException) {
                EditDescription(collection);
            }
            return false;
        }

        bool Wrap(CustomCollectionLibraryItemViewModel collection, Action act) {
            try {
                act();
                return true;
            } catch (CollectionEmptyException) {
                AddSomeItems();
            } catch (CollectionNameMissingException) {
                EditName(collection);
            } catch (CollectionDescriptionMissingException) {
                EditDescription(collection);
            }
            return false;
        }

        public Task<bool> ChangeScope(CustomCollectionLibraryItemViewModel collection, CollectionScope scope) {
            return WrapAsync(collection,
                () => _mediator.RequestAsyncWrapped(new ChangeCollectionScopeCommand(collection.Model.Id, scope)));
        }

        void AddSomeItems() {
            _dialogManager.MessageBoxSync(
                new MessageBoxDialogParams(
                    "You need to add some items to your collection before you can Publish it!",
                    "No mods in the collection"));
        }

        void EditDescription(CollectionLibraryItemViewModel collection) {
            _dialogManager.MessageBoxSync(
                new MessageBoxDialogParams(
                    "You need to add a description to your collection before you can Publish it!",
                    "Collection description missing"));
            collection.IsEditingDescription = true;
        }

        void EditName(CollectionLibraryItemViewModel collection) {
            _dialogManager.MessageBoxSync(
                new MessageBoxDialogParams(
                    "You need to name your collection before you can Publish it!",
                    "Collection name missing"));
            collection.IsEditing = true;
        }
    }
}