// <copyright company="SIX Networks GmbH" file="ISelectable.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}