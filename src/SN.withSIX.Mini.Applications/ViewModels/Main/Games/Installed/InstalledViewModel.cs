// <copyright company="SIX Networks GmbH" file="InstalledViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MoreLinq;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed
{
    public class InstalledViewModel : TabViewModel, IInstalledViewModel
    {
        readonly IReactiveCommand _clear;
        readonly Guid _id;
        readonly IReactiveCommand _playSelected;
        readonly ReactiveCommand<Unit> _uninstallSelected;

        public InstalledViewModel(Guid id, string metaDataSlug, IEnumerable<InstalledItemViewModel> localContent) {
            _id = id;
            LocalContent = new ReactiveList<InstalledItemViewModel>(localContent) {ChangeTrackingEnabled = true};
            LocalContent.ItemChanged
                .Where(x => x.PropertyName == "IsEnabled")
                .ObserveOnMainThread()
                .Subscribe(x => {
                    if (x.Sender.IsEnabled)
                        EnabledItems.Add(x.Sender);
                    else
                        EnabledItems.Remove(x.Sender);
                });
            EnabledItems = new ReactiveList<InstalledItemViewModel>(LocalContent.Where(x => x.IsEnabled));

            AddContent =
                ReactiveCommand.CreateAsyncTask(
                    async x => await RequestAsync(new OpenWebLink(ViewType.Browse, metaDataSlug)).ConfigureAwait(false));
            _uninstallSelected =
                ReactiveCommand.CreateAsyncTask(async x => {
                    var r =
                        await
                            Cheat.DialogManager.MessageBoxAsync(
                                new MessageBoxDialogParams("Are you sure you wish to uninstall the selected mods?",
                                    "Uninstall items?", SixMessageBoxButton.OKCancel)).ConfigureAwait(false);
                    if (r != SixMessageBoxResult.OK)
                        return;
                    await RequestAsync(
                        new UninstallInstalledItems(id,
                            LocalContent.Where(x1 => x1.IsEnabled).Select(x2 => x2.Id).ToList()))
                        .ConfigureAwait(false);
                }).DefaultSetup("UninstallSelected");

            _playSelected =
                ReactiveCommand.CreateAsyncTask(async x => await RequestAsync(
                    new PlayInstalledItems(id,
                        LocalContent.Where(x1 => x1.IsEnabled).Select(x2 => x2.Id).ToList()))
                    .ConfigureAwait(false))
                    .DefaultSetup("PlaySelected");
            _clear =
                ReactiveCommand.CreateAsyncTask(async x => await ResetInternal().ConfigureAwait(false))
                    .DefaultSetup("Reset");

            // TODO: This is a tab, and tabs are only active while shown
            // but we want to receive these events regardless of being active or not, otherwise we are not uptodate when the user switches to us.
            // Or we need to find a different approach!
            Listen<LocalContentAdded>()
                .Where(x => _id == x.GameId)
                .Select(x => x.LocalContent.MapTo<List<InstalledItemViewModel>>())
                .ObserveOnMainThread()
                .Subscribe(x => LocalContent.AddRange(x));
            Listen<UninstallActionCompleted>()
                .Where(x => _id == x.Game.Id)
                .Select(x => x.UninstallLocalContentAction.Content.Select(u => u.Content.Id).ToArray())
                .ObserveOnMainThread()
                .Subscribe(
                    x =>
                        LocalContent.RemoveAll(
                            LocalContent.Where(
                                c => x.Contains(c.Id))
                                .ToArray()));
            /*
            this.WhenActivated(d => {
                d();
            });
            */
        }

        public ICommand UninstallSelected => _uninstallSelected;
        public override string DisplayName => "My Content";
        public override string Icon => SixIconFont.withSIX_icon_System;
        public ReactiveList<InstalledItemViewModel> LocalContent { get; }
        public IReactiveCommand<UnitType> AddContent { get; }
        public ReactiveList<InstalledItemViewModel> EnabledItems { get; }
        public ICommand PlaySelected => _playSelected;
        public ICommand Clear => _clear;

        async Task ResetInternal() {
            SetLocalItemsIsEnabled(false);
        }

        void SetLocalItemsIsEnabled(bool isEnabled) {
            LocalContent.ForEach(x => x.IsEnabled = isEnabled);
        }
    }

    public interface IInstalledViewModel : IGameTabViewModel
    {
        ICommand PlaySelected { get; }
        ICommand Clear { get; }
        IReactiveCommand<UnitType> AddContent { get; }
        ReactiveList<InstalledItemViewModel> EnabledItems { get; }
        ReactiveList<InstalledItemViewModel> LocalContent { get; }
        ICommand UninstallSelected { get; }
    }
}