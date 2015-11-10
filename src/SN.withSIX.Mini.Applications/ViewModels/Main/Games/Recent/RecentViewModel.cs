// <copyright company="SIX Networks GmbH" file="RecentViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent
{
    public class RecentViewModel : TabViewModel, IRecentViewModel
    {
        readonly Guid _id;

        Guid GetId(Content content) {
            var local = content as LocalContent;
            return local == null || local.ContentId == Guid.Empty ? content.Id : local.ContentId;
        }

        public RecentViewModel(Guid id, string metaDataSlug, IEnumerable<RecentItemViewModel> recentItems) {
            _id = id;
            RecentItems = new ReactiveList<RecentItemViewModel>(recentItems);

            // TODO: This is a tab, and tabs are only active while shown
            // but we want to receive these events regardless of being active or not, otherwise we are not uptodate when the user switches to us.
            // Or we need to find a different approach!
            Listen<ContentUsed>()
                .Where(x => {
                    var contentId = GetId(x.Content);
                    lock (RecentItems)
                        return _id == x.Content.GameId && RecentItems.All(r => r.Id != contentId);
                })
                .Select(x => {
                    var ri = x.Content.MapTo<RecentItemViewModel>();
                    if (x.Token != null)
                        ri.UpdateExecute(x.Token);
                    return ri;
                })
                .ObserveOnMainThread()
                .Subscribe(x => {
                    lock (RecentItems)
                        RecentItems.Insert(0, x);
                });

            // TODO: Stop manually moving, start auto sorting in View (ICollectionView or ReactiveDerivedCollection)...
            // Then remove this event
            Listen<ContentUsed>()
                .Select(x => {
                    var contentId = GetId(x.Content);
                    lock (RecentItems)
                        return RecentItems.FirstOrDefault(r => r.Id == contentId);
                })
                .Where(x => x != null)
                .ObserveOnMainThread()
                .Subscribe(x => {
                    lock (RecentItems)
                        RecentItems.Move(RecentItems.IndexOf(x), 0);
                });

            AddContent =
                ReactiveCommand.CreateAsyncTask(
                    async x => await RequestAsync(new OpenWebLink(ViewType.Browse, metaDataSlug)).ConfigureAwait(false));


            this.WhenActivated(d => {
                RefreshUpdated();
                d(new TimerWithoutOverlap(TimeSpan.FromMinutes(1),
                    () => RxApp.MainThreadScheduler.Schedule(RefreshUpdated)));
            });
        }

        public IReactiveCommand<UnitType> AddContent { get; }

        public override string DisplayName => "Recent";
        public override string Icon => SixIconFont.withSIX_icon_Clock;
        public ReactiveList<RecentItemViewModel> RecentItems { get; }

        void RefreshUpdated() {
            foreach (var r in RecentItems)
                r.RaisePlayedUpdated();
        }
    }

    public interface IRecentViewModel : IGameTabViewModel
    {
        ReactiveList<RecentItemViewModel> RecentItems { get; }
        IReactiveCommand<UnitType> AddContent { get; }
    }

    public interface IGameTabViewModel : ITabViewModel {}
}