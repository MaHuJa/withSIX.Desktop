// <copyright company="SIX Networks GmbH" file="IModalScreen.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Caliburn.Micro;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public interface IShowBackButton
    {
        bool ShowBackButton { get; }
    }

    [DoNotObfuscate]
    public interface IModalScreen : IScreen, IShowBackButton
    {
        object Parent { get; set; }
        void Cancel();
    }
}