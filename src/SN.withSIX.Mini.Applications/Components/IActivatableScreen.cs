// <copyright company="SIX Networks GmbH" file="IActivatableScreen.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;

namespace SN.withSIX.Mini.Applications.Components
{
    public interface IActivatableScreen
    {
        bool IsOpen { get; set; }
        ReactiveCommand<object> Activate { get; }
        ReactiveCommand<bool?> Close { get; }
    }
}