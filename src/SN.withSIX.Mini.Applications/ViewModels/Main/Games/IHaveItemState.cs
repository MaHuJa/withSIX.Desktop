// <copyright company="SIX Networks GmbH" file="IHaveItemState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games
{
    public interface IHaveItemState
    {
        ItemState State { get; }
    }
}