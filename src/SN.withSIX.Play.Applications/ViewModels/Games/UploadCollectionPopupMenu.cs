// <copyright company="SIX Networks GmbH" file="UploadCollectionPopupMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class UploadCollectionPopupMenu : PopupMenuBase<CustomCollectionLibraryItemViewModel>
    {
        readonly UploadCollection _uploadCollection;

        public UploadCollectionPopupMenu(UploadCollection uploadCollection) {
            _uploadCollection = uploadCollection;
            Header = "Upload Collection";
        }

        [MenuItem, DoNotObfuscate]
        public Task Private(CustomCollectionLibraryItemViewModel item) {
            return _uploadCollection.Publish(item, CollectionScope.Private);
        }

        [MenuItem, DoNotObfuscate]
        public Task Unlisted(CustomCollectionLibraryItemViewModel item) {
            return _uploadCollection.Publish(item, CollectionScope.Unlisted);
        }

        [MenuItem, DoNotObfuscate]
        public Task Public(CustomCollectionLibraryItemViewModel item) {
            return _uploadCollection.Publish(item, CollectionScope.Public);
        }
    }
}