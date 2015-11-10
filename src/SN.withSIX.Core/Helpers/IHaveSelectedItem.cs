// <copyright company="SIX Networks GmbH" file="IHaveSelectedItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface IHaveSelectedItem<T>
    {
        T SelectedItem { get; set; }
    }
}