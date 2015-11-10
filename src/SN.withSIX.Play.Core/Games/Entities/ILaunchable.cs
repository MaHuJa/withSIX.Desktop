// <copyright company="SIX Networks GmbH" file="ILaunchable.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Play.Core.Games.Entities
{
    [ContractClass(typeof (LaunchableContract))]
    public interface ILaunchable
    {
        RunningGame Running { get; }
        Task<int> Launch(IGameLauncherFactory factory);
        void RegisterRunning(Process process);
        void RegisterTermination();
    }

    [ContractClassFor(typeof (ILaunchable))]
    public abstract class LaunchableContract : ILaunchable
    {
        public Task<int> Launch(IGameLauncherFactory factory) {
            Contract.Requires<ArgumentNullException>(factory != null);
            return default(Task<int>);
        }

        public abstract RunningGame Running { get; }

        public void RegisterRunning(Process process) {
            Contract.Requires<ArgumentNullException>(process != null);
        }

        public abstract void RegisterTermination();
    }
}