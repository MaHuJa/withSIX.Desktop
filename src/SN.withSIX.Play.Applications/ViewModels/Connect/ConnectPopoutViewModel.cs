// <copyright company="SIX Networks GmbH" file="ConnectPopoutViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    [DoNotObfuscate]
    public class ConnectPopoutViewModel : ScreenBase, ITransient
    {
        public ConnectPopoutViewModel(ConnectViewModel connect) {
            Connect = connect;
            DisplayName = "Connect withSIX";
        }

        public ConnectViewModel Connect { get; private set; }
    }
}