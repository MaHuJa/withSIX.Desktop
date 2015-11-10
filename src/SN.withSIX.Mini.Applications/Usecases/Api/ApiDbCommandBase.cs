// <copyright company="SIX Networks GmbH" file="ApiGameRequestBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public abstract class ApiDbQueryBase : DbQueryBase
    {
        protected ApiDbQueryBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        /// <summary>
        ///     Temporary measure to have the UI change to the game based on the action provided over the API..
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        protected static Task RaiseGameEvent(Game game) {
            return new ApiGameSelected(game).RaiseEvent();
        }
    }

    public abstract class ApiDbCommandBase : DbCommandBase
    {
        protected ApiDbCommandBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        /// <summary>
        ///     Temporary measure to have the UI change to the game based on the action provided over the API..
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        protected static Task RaiseGameEvent(Game game) {
            return new ApiGameSelected(game).RaiseEvent();
        }
    }
}