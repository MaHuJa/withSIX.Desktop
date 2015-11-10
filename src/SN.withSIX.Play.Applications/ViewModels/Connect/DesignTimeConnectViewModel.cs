// <copyright company="SIX Networks GmbH" file="DesignTimeConnectViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Caliburn.Micro;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    public class DesignTimeConnectViewModel : ConnectViewModel, IDesignTimeViewModel
    {
        public DesignTimeConnectViewModel()
            : base(
                IoC.Get<ContactList>(), null, null, null,
                IoC.Get<UserSettings>(),
                IoC.Get<IEventAggregator>()) {}
    }
}