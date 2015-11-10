// <copyright company="SIX Networks GmbH" file="HomeWorld2GameController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities.Other;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class HomeWorld2GameController : GameController<Homeworld2Game, HomeWorld2GameData>,
        ISupportProcessableContent<HomeWorld2GameData>, ISupportLaunchableContent<Homeworld2LaunchGlobalState>
    {
        public HomeWorld2GameController(Homeworld2Game game) : base(game) {}

        // TODO: Evaluate if it is problematic in any way that we have lost the specific type info of Collection, Mission, or Mod,
        // and are only left with ILaunchableContent<T> and IProcessableContent<T>

        // This can be called with multiple items of different types (collection, mission, mod etc)
        public Task Launch(IEnumerable<ILaunchableContent<Homeworld2LaunchGlobalState>> items, IMediator mediator) {
            var sharedState = new Homeworld2LaunchGlobalState();

            var resolvedItems = GetDependencyGraph(items);

            foreach (var launchableContent in resolvedItems) {
                //TODO: Do Exception Handling
                launchableContent.MakeLaunchState(sharedState, mediator); //Or do other action.
            }

            //TODO: Resolve Conflicts (To implement in the future)

            return Game.Launch(new Homeworld2StartupParameters(), mediator);
        }

        // This can only be called with a single item of different types (collection, mission, mod etc)
        // TODO: Shouldn't this take an IEnumerable just like Launch?
        public Task Install(IProcessableContent<HomeWorld2GameData> item, IMediator mediator) {
            return item.Install(GetGameData(), mediator);
        }

        public Task Uninstall(IProcessableContent<HomeWorld2GameData> item, IMediator mediator) {
            return item.Uninstall(GetGameData(), mediator);
        }

        public Task Verify(IProcessableContent<HomeWorld2GameData> item, IMediator mediator) {
            return item.Verify(GetGameData(), mediator);
        }

        static IEnumerable<ILaunchableContent<Homeworld2LaunchGlobalState>> GetDependencyGraph(
            IEnumerable<ILaunchableContent<Homeworld2LaunchGlobalState>> items) {
            var resolved = new List<ILaunchableContent<Homeworld2LaunchGlobalState>>();

            foreach (var i in items) {
                foreach (var d in i.Dependencies)
                    resolved.Add(d);
                resolved.Add(i);
            }
            return resolved;
        }

        //method 5
        // Receiveel ist of launchables
        // Order dependencies
        // Clean slate of game mods
        // foreach dependency execute Install, execute MakeLaunchState (or the mod would call its own Install method here...)
        // Pass on to game and launch
        // Arma state object example
        /*
        class ArmaLaunchableState
        {
            public string Mission;
            public List<string> Mods;
            public Dictionary<string, string> Additionalparameters;
        }*/

        //method 4
        // Receiveel ist of launchables
        // Order dependencies
        // Clean slate of game mods
        // foreach dependency execute Install
        // foreach dependency execute MakeLaunchState (or the mod would call its own Install method here...)
        // Pass on to game and launch

        //method 3
        // Recieve list of launchables
        // Order the list by dependencies
        // call each launchable to preform actions and return their state
        // Merge the states and store a list of conflicts if any
        // If any conflicts inform the user and request their resolution of which to pick (maybe provide option to save this for the future)
        // If conflicts existed merge the appropriate changes based on the users input
        // return game.Launch(state, mediator)

        // method2
        // Receive list of launchables
        // Setup the appropriate parameters/etc
        // return game.Launch(parameters, mediator)

        // method1
        // perform any install/uninstall/clean actions
        // set the state of the system to include launching this mod
        // return game.Launch(mediator)

        protected override HomeWorld2GameData GetGameData() {
            var installedState = Game.InstalledState;
            return new HomeWorld2GameData {
                Directory = installedState.Directory,
                ModPaths = Game.ModPaths
            };
        }
    }

    public abstract class LaunchableSomething<TLaunchState>
    {
        protected LaunchableSomething() {
            LaunchStates = new List<TLaunchState>();
        }

        protected List<TLaunchState> LaunchStates { get; set; }

        //TODO: Monitor Mutliple calls from same mod.
        public virtual void Add(TLaunchState homeworld2LaunchState) {
            LaunchStates.Add(homeworld2LaunchState);
        }

        //Add Exception Function

        //Add Conflict Resolution Function
    }

    public class Homeworld2LaunchGlobalState : LaunchableSomething<Homeworld2LaunchState> {}

    public interface ISharedLaunchableState<TLaunchableState>
        where TLaunchableState : ILaunchableState {}

    public interface ILaunchableState
    {
        Func<dynamic, bool> PreStateFunction { get; set; }
    }

    public class Homeworld2SharedLaunchState : ISharedLaunchableState<Homeworld2LaunchableState>
    {
        //public List<KeyValuePair<ILaunchableContent<Homeworld2SharedLaunchState, Homeworld2LaunchableState>, Homeworld2LaunchableState>> DependencyGraph
        //{ get; set; }
        public List<string> Mods { get; set; }
        public bool OverrideBigFiles { get; set; }
    }


    public class Homeworld2LaunchableState : ILaunchableState
    {
        public Func<dynamic, bool> PreStateFunction { get; set; }
    }

    public class Homeworld2Mod : Mod<HomeWorld2GameData, Homeworld2LaunchGlobalState>
    {
        public Homeworld2Mod(Guid id, PackageItem package, ModMetaData metaData) : base(id, package, metaData) {}

        protected override void DeleteAsync() {
            throw new NotImplementedException();
        }

        protected override ContentInstalledState GetInstalledState(HomeWorld2GameData gameData) {
            throw new NotImplementedException();
        }

        public override void MakeLaunchState(Homeworld2LaunchGlobalState sharedLaunchState,
            IMediator domainMediator) {
            sharedLaunchState.Add(new Homeworld2LaunchState());
        }
    }

    public class LaunchState
    {
        //ConflictResolutionIssues
        //Excpetions
    }

    public class Homeworld2LaunchState : LaunchState
    {
        //command parameters (-overrideBig)
        //IsLaunchable
        //List of KeyValue<string,Func<DataType,DataType>> commands;
        // - new KV("overrideBig", (value) => {if(value == false && value != null) throw Conflict();})
    }

    public class Arma3LaunchState : LaunchState
    {
        //Parameters
        //Mod Location
    }
}

/*
 * - Run a mod
 * -- Adds a test expression to a list
 * -- Changes parameter
 * - Run next mod
 * -- Adds a test expression to the list
 * -- Changes parameter
 * --- 1st mod test expression is still run on new value and can throw a conflict.
 * 
 */