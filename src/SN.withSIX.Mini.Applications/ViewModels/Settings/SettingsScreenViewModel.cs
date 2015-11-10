// <copyright company="SIX Networks GmbH" file="SettingsScreenViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public abstract class SettingsScreenViewModel : ScreenViewModel, ISettingsScreenViewModel
    {
        protected SettingsScreenViewModel() {
            Ok =
                ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.Valid),
                    async x => await SaveSettings().ConfigureAwait(false))
                    .DefaultSetup("Ok");
            Cancel = ReactiveCommand.Create().DefaultSetup("Cancel");
        }

        public abstract bool Valid { get; }
        public ReactiveCommand<Unit> Ok { get; }
        public ReactiveCommand<object> Cancel { get; }
        protected abstract Task SaveSettings();
    }

    public interface ISettingsScreenViewModel : IScreenViewModel, IHaveOkCancel {}

    public interface IHaveOkCancel
    {
        ReactiveCommand<Unit> Ok { get; }
        ReactiveCommand<object> Cancel { get; }
    }
}