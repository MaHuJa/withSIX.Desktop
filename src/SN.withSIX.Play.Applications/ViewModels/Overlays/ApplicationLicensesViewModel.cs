// <copyright company="SIX Networks GmbH" file="ApplicationLicensesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Infrastructure;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    [DoNotObfuscate]
    public class ApplicationLicensesViewModel : OverlayViewModelBase
    {
        public ApplicationLicensesViewModel(IResourceService resources) {
            DisplayName = "Application Licenses";

            string[] licenses;
            using (var stream = resources.GetResource("applicenses.applicenses.lst")
                )
            using (var reader = new StreamReader(stream))
                licenses = reader.ReadToEnd().Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            var result = "";
            foreach (var resource in licenses) {
                using (var stream = resources.GetResource("applicenses." + resource)
                    )
                using (var reader = new StreamReader(stream))
                    result = result + reader.ReadToEnd() + "\n\n\n";
            }
            LicenseText = result;
        }

        public string LicenseText { get; private set; }
    }
}