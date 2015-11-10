// <copyright company="SIX Networks GmbH" file="ISearchLibraryItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public interface ISearchLibraryItem<T> : ISelectionList<T> where T : class
    {
        void UpdateItems(IEnumerable<T> items);
    }
}