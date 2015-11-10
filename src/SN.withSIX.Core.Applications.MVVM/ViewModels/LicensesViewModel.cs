// <copyright company="SIX Networks GmbH" file="LicensesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public interface ILicensesViewModel : IModalScreen {}

    [DoNotObfuscate]
    public class LicensesViewModel : ReactiveModalScreen<IShellViewModelBase>, ILicensesViewModel
    {
        public LicensesViewModel(Licenses licenses) {
            DisplayName = "licenses";

            Licenses = licenses;
        }

        public Licenses Licenses { get; private set; }
    }
}