// <copyright company="SIX Networks GmbH" file="SettingsTab.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public abstract class SettingsTabViewModel : ValidatableViewModel, ISettingsTabViewModel
    {
        public virtual IEnumerable<ISettingsTabViewModel> SubItems { get; } = Enumerable.Empty<ISettingsTabViewModel>();
    }

    public interface ISettingsTabViewModel : IValidatableViewModel
    {
        IEnumerable<ISettingsTabViewModel> SubItems { get; }
    }
}