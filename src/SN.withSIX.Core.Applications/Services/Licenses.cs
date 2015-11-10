// <copyright company="SIX Networks GmbH" file="Licenses.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Infrastructure;

namespace SN.withSIX.Core.Applications.Services
{
    [DoNotObfuscate]
    public class Licenses : IApplicationService
    {
        public Licenses(IResourceService resourceService) {
            string[] licenses;
            using (var stream = resourceService.GetResource("applicenses.applicenses.lst")
                )
            using (var reader = new StreamReader(stream))
                licenses = reader.ReadToEnd().Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            var result = "";
            foreach (var resource in licenses) {
                using (var stream = resourceService.GetResource("applicenses." + resource))
                using (var reader = new StreamReader(stream))
                    result = result + reader.ReadToEnd() + "\n\n\n";
            }
            LicenseText = result;
        }

        public string LicenseText { get; private set; }
    }
}