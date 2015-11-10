// <copyright company="SIX Networks GmbH" file="AutoMapperAppConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Favorite;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Installed;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications
{
    public class AutoMapperAppConfig
    {
        public static void Setup() {
            SetupGamesView();
            SetupSettingsTabs();
            SetupApi();
        }

        static void SetupApi() {
            Cheat.MapperConfiguration.CreateMap<Game, ClientContentInfo2>()
                .ForMember(x => x.FavoriteContent, opt => opt.MapFrom(src => src.FavoriteItems))
                .ForMember(x => x.InstalledContent, opt => opt.MapFrom(src => src.LocalContent))
                .ForMember(x => x.RecentContent,
                    opt => opt.MapFrom(src => src.RecentItems.OrderByDescending(x => x.RecentInfo.LastUsed).Take(10)));

            Cheat.MapperConfiguration.CreateMap<LocalContent, InstalledContentModel>();

            Cheat.MapperConfiguration.CreateMap<LocalCollection, LocalCollectionModel>();

            Cheat.MapperConfiguration.CreateMap<Content, ContentModel>()
                .Include<LocalContent, ContentModel>()
                .Include<Collection, ContentModel>()
                .Include<LocalCollection, ContentModel>()
                .Include<NetworkCollection, ContentModel>()
                .Include<SubscribedCollection, ContentModel>()
                .Include<NetworkContent, ContentModel>()
                .Include<ModNetworkContent, ContentModel>()
                .Include<MissionNetworkContent, ContentModel>();

            Cheat.MapperConfiguration.CreateMap<LocalContent, ContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkContent, ContentModel>()
                .Include<ModNetworkContent, ContentModel>()
                .Include<MissionNetworkContent, ContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkCollection, ContentModel>()
                .Include<SubscribedCollection, ContentModel>();

            Cheat.MapperConfiguration.CreateMap<Content, RecentContentModel>()
                .Include<LocalContent, RecentContentModel>()
                .Include<Collection, RecentContentModel>()
                .Include<LocalCollection, RecentContentModel>()
                .Include<NetworkCollection, RecentContentModel>()
                .Include<SubscribedCollection, RecentContentModel>()
                .Include<NetworkContent, RecentContentModel>()
                .Include<ModNetworkContent, RecentContentModel>()
                .Include<MissionNetworkContent, RecentContentModel>();

            Cheat.MapperConfiguration.CreateMap<LocalContent, RecentContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkContent, RecentContentModel>()
                .Include<ModNetworkContent, RecentContentModel>()
                .Include<MissionNetworkContent, RecentContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkCollection, RecentContentModel>()
                .Include<SubscribedCollection, RecentContentModel>();

            Cheat.MapperConfiguration.CreateMap<Content, FavoriteContentModel>()
                .Include<LocalContent, FavoriteContentModel>()
                .Include<Collection, FavoriteContentModel>()
                .Include<LocalCollection, FavoriteContentModel>()
                .Include<NetworkCollection, FavoriteContentModel>()
                .Include<SubscribedCollection, FavoriteContentModel>()
                .Include<NetworkContent, FavoriteContentModel>()
                .Include<ModNetworkContent, FavoriteContentModel>()
                .Include<MissionNetworkContent, FavoriteContentModel>();

            Cheat.MapperConfiguration.CreateMap<LocalContent, FavoriteContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkContent, FavoriteContentModel>()
                .Include<ModNetworkContent, FavoriteContentModel>()
                .Include<MissionNetworkContent, FavoriteContentModel>();
            Cheat.MapperConfiguration.CreateMap<NetworkCollection, FavoriteContentModel>()
                .Include<SubscribedCollection, FavoriteContentModel>();

            Cheat.MapperConfiguration.CreateMap<ContentStatusChanged, ContentState>();
            Cheat.MapperConfiguration.CreateMap<Content, ContentState>()
                .Include<LocalContent, ContentState>()
                .Include<Collection, ContentState>()
                .Include<NetworkCollection, ContentState>();
            // TODO: For collections we should look at the contents of the collection, and then determine if all the content is installed/uptodate
            // then determine the collection state based on that. for this we can't use automapper. We will have to manually do this by providing the games and checking their installed content.
            // Perhaps we should then cache this state too..
            Cheat.MapperConfiguration.CreateMap<NetworkCollection, ContentState>();
            Cheat.MapperConfiguration.CreateMap<LocalContent, ContentState>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ContentId == Guid.Empty ? src.Id : src.ContentId));
            Cheat.MapperConfiguration.CreateMap<Game, GameApiModel>()
                // .Include<Game, GameHomeApiModel>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Metadata.Name))
                .ForMember(x => x.Slug, opt => opt.MapFrom(src => src.Metadata.Slug))
                .ForMember(x => x.Image, opt => opt.MapFrom(src => src.Metadata.Image))
                .ForMember(x => x.CollectionsCount, opt => opt.MapFrom(src => src.LocalCollections.Count()))
                .ForMember(x => x.MissionsCount,
                    opt => opt.MapFrom(src => src.LocalContent.OfType<MissionLocalContent>().Count()))
                .ForMember(x => x.ModsCount,
                    opt => opt.MapFrom(src => src.LocalContent.OfType<ModLocalContent>().Count()));
            //.ForMember(x => x.Author, opt => opt.MapFrom(src => src.Metadata.Author));

            Cheat.MapperConfiguration.CreateMap<Content, ContentApiModel>()
                .ForMember(x => x.Type, opt => opt.MapFrom(src => ConvertToType(src)));
            Cheat.MapperConfiguration.CreateMap<ModLocalContent, ContentApiModel>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ContentId != Guid.Empty ? src.ContentId : src.Id))
                .ForMember(x => x.Type, opt => opt.MapFrom(src => ConvertToType(src)));
            Cheat.MapperConfiguration.CreateMap<MissionLocalContent, ContentApiModel>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ContentId != Guid.Empty ? src.ContentId : src.Id))
                .ForMember(x => x.Type, opt => opt.MapFrom(src => ConvertToType(src)));
            Cheat.MapperConfiguration.CreateMap<LocalCollection, ContentApiModel>()
                .ForMember(x => x.Type, opt => opt.MapFrom(src => ConvertToType(src)));

            Cheat.MapperConfiguration.CreateMap<List<Game>, GamesApiModel>()
                .ForMember(x => x.Games, opt => opt.MapFrom(src => src.Select(x => x.MapTo<GameApiModel>()).ToList()));

            Cheat.MapperConfiguration.CreateMap<List<Game>, HomeApiModel>()
                .ForMember(x => x.Games, opt => opt.MapFrom(src => src.Select(x => x.MapTo<GameApiModel>()).ToList()))
                .ForMember(x => x.Recent,
                    opt => opt.MapFrom(src => src.SelectMany(x => x.RecentItems.Select(c => new {x, c}))
                        .OrderByDescending(x => x.c.RecentInfo.LastUsed)
                        .Take(10).Select(x => Convert(x.x, x.c))))
                .ForMember(x => x.NewContent,
                    opt => opt.MapFrom(src => src.SelectMany(x => x.LocalContent.Select(c => new {x, c}))
                        .OrderByDescending(x => x.c.InstallInfo.LastInstalled)
                        .Take(10).Select(x => Convert(x.x, x.c))))
                .ForMember(x => x.Updates,
                    opt => opt.MapFrom(src => src.SelectMany(x => x.Updates.Take(10).Select(c => new {x, c}))
                        .Take(10).Select(x => Convert(x.x, x.c))));

            Cheat.MapperConfiguration.CreateMap<Game, GameHomeApiModel>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Metadata.Name))
                .ForMember(x => x.Slug, opt => opt.MapFrom(src => src.Metadata.Slug))
                .ForMember(x => x.Image, opt => opt.MapFrom(src => src.Metadata.Image))
                .ForMember(x => x.Recent, opt => opt.MapFrom(src => src.RecentItems
                    .OrderByDescending(x => x.RecentInfo.LastUsed)
                    .Take(10).Select(x => Convert(src, x))))
                .ForMember(x => x.NewContent, opt => opt.MapFrom(src => src.LocalContent
                    .OrderByDescending(x => x.InstallInfo.LastInstalled)
                    .Take(10).Select(x => Convert(src, x))))
                .ForMember(x => x.Updates, opt => opt.MapFrom(src => src.Updates
                    .Take(10).Select(x => Convert(src, x)).ToList()));

            Cheat.MapperConfiguration.CreateMap<Game, GameMissionsApiModel>()
                .ForMember(x => x.Missions,
                    opt => opt.MapFrom(src => src.Contents.OfType<MissionLocalContent>().Select(x => Convert(src, x))));
            Cheat.MapperConfiguration.CreateMap<Game, GameModsApiModel>()
                .ForMember(x => x.Mods,
                    opt => opt.MapFrom(src => src.Contents.OfType<ModLocalContent>().Select(x => Convert(src, x))));
            Cheat.MapperConfiguration.CreateMap<Game, GameCollectionsApiModel>()
                .ForMember(x => x.Collections,
                    opt => opt.MapFrom(src => src.Contents.OfType<LocalCollection>().Select(x => Convert(src, x))));
        }

        static string ConvertToType(Content src) {
            // TODO: expand types.. missions etc
            // Or investigate reimplementation of ContentSlug, based on new ContentType that is available on every class, not just network content?
            return src is Collection ? "collection" : "mod";
        }

        static ContentApiModel Convert<T>(Game game, T item) where T : Content {
            var convert = item.MapTo<ContentApiModel>();
            convert.GameId = game.Id;
            convert.GameSlug = game.Metadata.Slug;
            return convert;
        }

        static void SetupGamesView() {
            Cheat.MapperConfiguration.CreateMap<Game, GameItemViewModel>()
                .ConstructUsing(
                    src =>
                        new GameItemViewModel(src.Id,
                            src.Metadata.LaunchTypes.ToSelectionCollectionHelper(src.LastUsedLaunchType)))
                .ForMember(x => x.Slug, opt => opt.MapFrom(src => src.Metadata.Slug))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Metadata.Name))
                .ForMember(x => x.Image, opt => opt.MapFrom(src => src.Metadata.Image))
                .ForMember(x => x.BackgroundImage, opt => opt.MapFrom(src => src.Metadata.BackgroundImage))
                .ForMember(x => x.IsInstalled, opt => opt.MapFrom(src => src.InstalledState.IsInstalled))
                .ForMember(x => x.LaunchTypes,
                    opt =>
                        opt.ResolveUsing(
                            src => src.Metadata.LaunchTypes.ToSelectionCollectionHelper(src.LastUsedLaunchType)));
            /*
            Cheat.MapperConfiguration.CreateMap<Game, IGameItemViewModel>()
                .As<GameItemViewModel>();
*/

            SetupGameView();
        }

        static void SetupGameView() {
            Cheat.MapperConfiguration.CreateMap<Game, GameViewModel>()
                .ForMember(x => x.FirstTimeRunInfo, opt => opt.MapFrom(src => src.Metadata.FirstTimeRunInfo))
                .ConstructUsing(src => {
                    var viewModels = new IGameTabViewModel[]
                    {src.MapTo<RecentViewModel>(), src.MapTo<FavoriteViewModel>(), src.MapTo<InstalledViewModel>()};
                    return new GameViewModel(src.Id,
                        new SelectionCollectionHelper<IGameTabViewModel>(viewModels, viewModels.First()));
                });
            /*
            Cheat.MapperConfiguration.CreateMap<Game, IGameViewModel>()
                .As<GameViewModel>();
*/

            SetupFavoriteView();
            SetupRecentView();
            SetupInstalledView();
        }

        static void SetupFavoriteView() {
            Cheat.MapperConfiguration.CreateMap<Game, FavoriteViewModel>()
                .ForMember(x => x.FavoriteItems, opt => opt.MapFrom(src => src.FavoriteItems.OrderBy(x => x.Name)));
            /*
            Cheat.MapperConfiguration.CreateMap<Game, IFavoriteViewModel>()
                .As<FavoriteViewModel>();
*/

            Cheat.MapperConfiguration.CreateMap<Content, FavoriteItemViewModel>()
                .ConstructUsing(
                    x =>
                        new FavoriteItemViewModel(x.Id, x.GameId, x.Name, x.Image,
                            x is IHavePath, x is LocalContent,
                            new SelectionCollectionHelper<PlayAction>(new[] {PlayAction.Play}, PlayAction.Play)))
                .Include<LocalContent, FavoriteItemViewModel>()
                .Include<Collection, FavoriteItemViewModel>()
                .Include<LocalCollection, FavoriteItemViewModel>()
                .Include<NetworkCollection, FavoriteItemViewModel>()
                .Include<SubscribedCollection, FavoriteItemViewModel>()
                .Include<NetworkContent, FavoriteItemViewModel>()
                .Include<ModNetworkContent, FavoriteItemViewModel>()
                .Include<MissionNetworkContent, FavoriteItemViewModel>();

            /*            Cheat.MapperConfiguration.CreateMap<Content, IFavoriteItemViewModel>()
                .Include<LocalContent, IFavoriteItemViewModel>()
                .Include<Collection, IFavoriteItemViewModel>()
                .Include<LocalCollection, IFavoriteItemViewModel>()
                .Include<NetworkCollection, IFavoriteItemViewModel>()
                .Include<SubscribedCollection, IFavoriteItemViewModel>()
                .Include<NetworkContent, IFavoriteItemViewModel>()
                .Include<ModNetworkContent, IFavoriteItemViewModel>()
                .Include<MissionNetworkContent, IFavoriteItemViewModel>()
                .As<FavoriteItemViewModel>();*/

            Cheat.MapperConfiguration.CreateMap<NetworkCollection, FavoriteItemViewModel>()
                .ConstructUsing(
                    x =>
                        new FavoriteItemViewModel(x.Id, x.GameId, x.Name, x.Image,
                            x is IHavePath, x is LocalContent, GetActions(x)))
                .Include<SubscribedCollection, FavoriteItemViewModel>();
        }

        static SelectionCollectionHelper<PlayAction> GetActions(NetworkCollection src) {
            return src.Servers.Any()
                ? new SelectionCollectionHelper<PlayAction>(
                    new[] {PlayAction.Join, PlayAction.Launch, PlayAction.Play}, PlayAction.Join)
                : new SelectionCollectionHelper<PlayAction>(new[] {PlayAction.Play, PlayAction.Launch}, PlayAction.Play);
        }

        static void SetupRecentView() {
            Cheat.MapperConfiguration.CreateMap<Game, RecentViewModel>()
                .ForMember(x => x.RecentItems,
                    opt => opt.MapFrom(src => src.RecentItems.OrderByDescending(x => x.RecentInfo.LastUsed)));
            /*
            Cheat.MapperConfiguration.CreateMap<Game, IRecentViewModel>()
                .As<RecentViewModel>();
*/
            Cheat.MapperConfiguration.CreateMap<Content, RecentItemViewModel>()
                .ConstructUsing(
                    x =>
                        new RecentItemViewModel(x.Id, x.GameId, x.Name, x.Image,
                            x is IHavePath, GetActions(x)))
                .ForMember(x => x.LastUsed, opt => opt.MapFrom(src => src.RecentInfo.LastUsed));
            // Bah
            //.ForMember(x => x.IsVisitable, opt => opt.MapFrom(src => src.Content.Content is IHavePath));
            /*            Cheat.MapperConfiguration.CreateMap<Content, IRecentItemViewModel>()
                .As<RecentItemViewModel>();*/
        }

        static ISelectionCollectionHelper<PlayAction> GetActions(Content src) {
            var nc = src as NetworkCollection;
            return nc == null ? new SelectionCollectionHelper<PlayAction>(new[] {PlayAction.Play}) : GetActions(nc);
        }

        static void SetupInstalledView() {
            Cheat.MapperConfiguration.CreateMap<Game, InstalledViewModel>()
                .ForMember(x => x.LocalContent, opt => opt.MapFrom(src => src.LocalContent.OrderBy(x => x.Name)));
            //Cheat.MapperConfiguration.CreateMap<Game, IInstalledViewModel>()
            //.As<InstalledViewModel>();
            Cheat.MapperConfiguration.CreateMap<LocalContent, InstalledItemViewModel>()
                .ConstructUsing(
                    x =>
                        new InstalledItemViewModel(x.Id, x.GameId, x.IsFavorite, x.Name, x.Version, x.Image,
                            x.Path != null))
                .Include<ModLocalContent, InstalledItemViewModel>()
                .Include<MissionLocalContent, InstalledItemViewModel>();
            /*
            Cheat.MapperConfiguration.CreateMap<LocalContent, IInstalledItemViewModel>()
                .Include<ModLocalContent, IInstalledItemViewModel>()
                .Include<MissionLocalContent, IInstalledItemViewModel>()
                .As<InstalledItemViewModel>();
*/
        }

        static void SetupSettingsTabs() {
            Cheat.MapperConfiguration.CreateMap<Settings, GeneralSettings>()
                .IgnoreAllMembers()
                .ForMember(x => x.OptOutErrorReports, opt => opt.MapFrom(src => src.Local.OptOutReporting))
                .ForMember(x => x.EnableDesktopNotifications,
                    opt => opt.MapFrom(src => src.Local.ShowDesktopNotifications))
                .ForMember(x => x.LaunchWithWindows, opt => opt.MapFrom(src => src.Local.StartWithWindows));
            Cheat.MapperConfiguration.CreateMap<GeneralSettings, Settings>()
                .IgnoreAllMembers()
                .AfterMap((src, dest) => src.MapTo(dest.Local));
            Cheat.MapperConfiguration.CreateMap<GeneralSettings, LocalSettings>()
                .IgnoreAllMembers()
                .ForMember(x => x.OptOutReporting, opt => opt.MapFrom(src => src.OptOutErrorReports))
                .ForMember(x => x.ShowDesktopNotifications,
                    opt => opt.MapFrom(src => src.EnableDesktopNotifications))
                .ForMember(x => x.StartWithWindows, opt => opt.MapFrom(src => src.LaunchWithWindows));

            Cheat.MapperConfiguration.CreateMap<Settings, GamesSettingsTabViewModel>()
                .IgnoreAllMembers();

            Cheat.MapperConfiguration.CreateMap<Settings, AccountSettingsTabViewModel>()
                .IgnoreAllMembers()
                .ForMember(x => x.LoginInfo, opt => opt.MapFrom(src => src.Secure.Login ?? LoginInfo.Default));

            //Cheat.MapperConfiguration.CreateMap<Settings, NotificationSettingsTabViewModel>()
            //.IgnoreAllMembers();
            //Cheat.MapperConfiguration.CreateMap<Settings, DownloadSettingsTabViewModel>()
            //.IgnoreAllMembers();
            Cheat.MapperConfiguration.CreateMap<Settings, InterfaceSettingsTabViewModel>()
                .IgnoreAllMembers()
                .ForMember(x => x.OptOutReporting, opt => opt.MapFrom(src => src.Local.OptOutReporting))
                .ForMember(x => x.ShowDesktopNotifications,
                    opt => opt.MapFrom(src => src.Local.ShowDesktopNotifications))
                .ForMember(x => x.StartWithWindows, opt => opt.MapFrom(src => src.Local.StartWithWindows));

            Cheat.MapperConfiguration.CreateMap<AccountSettingsTabViewModel, Settings>()
                .IgnoreAllMembers();
            //Cheat.MapperConfiguration.CreateMap<NotificationSettingsTabViewModel, Settings>()
            //.IgnoreAllMembers();
            //Cheat.MapperConfiguration.CreateMap<DownloadSettingsTabViewModel, Settings>()
            //.IgnoreAllMembers();
            Cheat.MapperConfiguration.CreateMap<InterfaceSettingsTabViewModel, Settings>()
                .IgnoreAllMembers()
                .AfterMap((s, d) => s.MapTo(d.Local));
            Cheat.MapperConfiguration.CreateMap<InterfaceSettingsTabViewModel, LocalSettings>()
                .IgnoreAllMembers()
                .ForMember(x => x.OptOutReporting, opt => opt.MapFrom(src => src.OptOutReporting))
                .ForMember(x => x.StartWithWindows, opt => opt.MapFrom(src => src.StartWithWindows))
                .ForMember(x => x.ShowDesktopNotifications, opt => opt.MapFrom(src => src.ShowDesktopNotifications));

            Cheat.MapperConfiguration.CreateMap<Game, DetectedGameItemViewModel>();
            /*
            Cheat.MapperConfiguration.CreateMap<Game, IDetectedGameItemViewModel>()
                .As<DetectedGameItemViewModel>();
*/
        }
    }
}