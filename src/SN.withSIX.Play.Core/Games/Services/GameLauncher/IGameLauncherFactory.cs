// <copyright company="SIX Networks GmbH" file="IGameLauncherFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Games.Services.GameLauncher
{
    [ContractClass(typeof (ContractClassForGameLauncherFactory))]
    public interface IGameLauncherFactory
    {
        T Create<T>(ILaunchWith<T> game) where T : class, IGameLauncher;
    }

    [ContractClassFor(typeof (IGameLauncherFactory))]
    public abstract class ContractClassForGameLauncherFactory : IGameLauncherFactory
    {
        public T Create<T>(ILaunchWith<T> game) where T : class, IGameLauncher {
            Contract.Requires<ArgumentNullException>(game != null);
            return default(T);
        }
    }
}