// <copyright company="SIX Networks GmbH" file="SettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Annotations;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Settings;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public class SettingsViewModel : SettingsScreenViewModel, ISettingsViewModel
    {
        static readonly string displayNameInternal = Cheat.WindowDisplayName("Settings");
        readonly ObservableAsPropertyHelper<bool> _valid;

        public SettingsViewModel(IEnumerable<ISettingsTabViewModel> settingsTabs) {
            var settings = settingsTabs.OrderBy(GetOrder).ToArray();
            Settings = new SelectionCollectionHelper<ISettingsTabViewModel>(settings) {
                SelectedItem = settings.FirstOrDefault()
            };
            _valid = settings.Select(x => x.WhenAnyValue(s => s.IsValid))
                .CombineLatest(x => x.All(b => b))
                .ToProperty(this, x => x.Valid);
        }

        public override bool Valid => _valid.Value;
        public override string DisplayName => displayNameInternal;
        public ISelectionCollectionHelper<ISettingsTabViewModel> Settings { get; }

        static int GetOrder(ISettingsTabViewModel x) {
            var orderAttribute =
                ((OrderAttribute) x.GetType().GetCustomAttributes(typeof (OrderAttribute)).SingleOrDefault());
            return orderAttribute?.Order ?? 9999;
        }

        protected override Task SaveSettings() {
            return
                RequestAsync(
                    new SaveSettings(Settings.Items.Concat(Settings.Items.SelectMany(x => x.SubItems)).ToArray()));
        }
    }

    public interface ISettingsViewModel : ISettingsScreenViewModel
    {
        ISelectionCollectionHelper<ISettingsTabViewModel> Settings { get; }
    }
}