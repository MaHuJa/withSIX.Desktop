// <copyright company="SIX Networks GmbH" file="UiNotificationHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Applications.NotificationHandlers
{
    // Domain Event approach
    // + Pretty loosely coupled
    // + Easily extendable - Add Event, and Add appropriate handler
    // + Can reach anywhere in the View hierarchy, not just the leaf that executed the command..
    // - How to deal with satellite windows or other things that are harder to reach?
    // - Requires access over singleton MainWindowViewModel, then TrayViewModel, then the actual data etc.
    // - Requires an array search in some cases... Will cause threading issues if that array can be modified while the command is running/finishing...
    // - Requires public setter for the property to be set [who cares]

    // Domain Event Aggregator approach on the Screens only (so not array items)
    // + Pretty loosely coupled
    // + Easily extendable - Add Event, and Add appropriate handler
    // + Can reach anywhere in the View hierarchy, not just the leaf that executed the command..
    // - ViewModel needs to Map Model to ViewModel..
    // - Requires an array search in some cases... Will cause threading issues if that array can be modified while the command is running/finishing...

    // Domain Event Aggregator approach on the Screens and array items (compared to the one above)
    // + Does not require an array search
    // - Now there are many more listeners for events..

    // Return data from commandhandler approach
    // + Seemingly the easiest and simplest solution
    // - Not loosely coupled
    // - Not easily extendable
    // - Requires extensive knowledge in the commandhandler to know what has changed, and thus what to return
    // - If the command changes data outside the reach of the ReactiveCommand that executed it, then there is no way to propagate those changes to where they need to end up.

    // Would a combination be useful?

    // TODO: Manually fiddling with indexes for sorting is a bit nasty, probably should use derrivedcollections or CollectionView ;-)
    /*    public class UiNotificationHandler : INotificationHandler<RecentItemPlayed>, INotificationHandler<RecentItemAdded>
        {
            readonly IMiniMainWindowViewModel _mainWindowViewModel;

            public UiNotificationHandler(IMiniMainWindowViewModel mainWindowViewModel) {
                _mainWindowViewModel = mainWindowViewModel;
            }

            public void Handle(RecentItemAdded notification) {
                var gameList = _mainWindowViewModel.TrayViewModel.List as IRecentViewModel;
                if (gameList != null && gameList.Game.Id == notification.RecentItem.GameId)
                    gameList.Game.RecentItems.Insert(0, notification.RecentItem.MapTo<RecentItemViewModel>());
            }

            public void Handle(RecentItemPlayed notification) {
                var gameList = _mainWindowViewModel.TrayViewModel.List as IRecentViewModel;
                if (gameList == null)
                    return;

                var recentItems = gameList.Game.RecentItems;
                var recentItem =
                    recentItems.FirstOrDefault(x => x.Id == notification.Recent.Id);
                if (recentItem == null)
                    return;
                recentItem.LastPlayed = notification.Recent.LastPlayed;
                recentItems.Move(recentItems.IndexOf(recentItem), 0);
            }
        }*/
}