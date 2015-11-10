// <copyright company="SIX Networks GmbH" file="ISupport.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    // TODO: Support cancellationtokens!
    public static class Extensions
    {
        public static ISupportCollections Collections(this Game game) {
            return (ISupportCollections) game;
        }

        public static ISupportMissions Missions(this Game game) {
            return (ISupportMissions) game;
        }

        public static ISupportModding Modding(this Game game) {
            return (ISupportModding) game;
        }
    }

    // These are used to deal with loss of type information in UI/App.
    // The idea is that the game knows its types, so by sending a universal ID to the game, it should be able to find the appropriate content of the appropriate type
    // and act accordingly on it.
    //
    // TODO: Should these support IEnumaberable?
    public interface ISupportCollections
    {
        Task Install(CollectionId id, IMediator mediator);
        Task Uninstall(CollectionId id, IMediator mediator);
        Task Verify(CollectionId id, IMediator mediator);
        Task Launch(CollectionId id, IMediator mediator);
    }

    public interface ISupportModding : SN.withSIX.Play.Core.Games.Entities.ISupportModding
    {
        Task Install(ModId id, IMediator mediator);
        Task Uninstall(ModId id, IMediator mediator);
        Task Verify(ModId id, IMediator mediator);
        Task Launch(ModId id, IMediator mediator);
    }

    public interface ISupportMissions : SN.withSIX.Play.Core.Games.Entities.ISupportMissions
    {
        Task Install(MissionId id, IMediator mediator);
        Task Uninstall(MissionId id, IMediator mediator);
        Task Verify(MissionId id, IMediator mediator);
        Task Launch(MissionId id, IMediator mediator);
    }


    // Used on Content objects
    public interface IProcessableContent<in TGameData>
    {
        Task Install(TGameData gameData, IMediator mediator);
        Task Uninstall(TGameData gameData, IMediator mediator);
        Task Verify(TGameData gameData, IMediator mediator);
    }

    // Used on Content objects
    public interface ILaunchableContent<in TLaunchSomething>
    {
        //TODO: Function to return our dependancy graph
        // TODO: The Dependencies probably need to go to a shared interface
        IEnumerable<ILaunchableContent<TLaunchSomething>> Dependencies { get; }
        // TODO: How to deal with Dependencies for Processing?
        void MakeLaunchState(TLaunchSomething sharedLaunchState, IMediator domainMediator);
    }

    // Used on Controller objects
    public interface ISupportProcessableContent<out TGameData>
    {
        Task Install(IProcessableContent<TGameData> item, IMediator mediator);
        Task Uninstall(IProcessableContent<TGameData> item, IMediator mediator);
        Task Verify(IProcessableContent<TGameData> item, IMediator mediator);
    }

    // Used on Controller objects
    public interface ISupportLaunchableContent<out TGameData>
    {
        Task Launch(IEnumerable<ILaunchableContent<TGameData>> items, IMediator mediator);
    }

    // Use in a ContentContainer (while Game can provide content containers? (e.g Game path, and Mod path))
    public interface ISupportListing<out TItem>
    {
        IReadOnlyCollection<TItem> List { get; }
    }

    // ???
    public interface IServers
    {
        Task Query(Server server);
        Task QueryServers();
    }
}