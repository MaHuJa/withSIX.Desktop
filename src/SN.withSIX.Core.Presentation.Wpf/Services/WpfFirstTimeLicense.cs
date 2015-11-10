// <copyright company="SIX Networks GmbH" file="WpfFirstTimeLicense.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Caliburn.Micro;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class WpfFirstTimeLicense : IFirstTimeLicense
    {
        readonly IWindowManager _windowManager;

        public WpfFirstTimeLicense(IWindowManager windowManager) {
            _windowManager = windowManager;
        }

        public bool ConfirmLicense(object obj) {
            return _windowManager.ShowDialog(obj).GetValueOrDefault(false);
        }
    }
}