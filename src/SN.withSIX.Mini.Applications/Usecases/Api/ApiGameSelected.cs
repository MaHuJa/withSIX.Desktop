// <copyright company="SIX Networks GmbH" file="ApiGameSelected.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class ApiGameSelected : IDomainEvent
    {
        public ApiGameSelected(Game game) {
            Game = game;
        }

        public Game Game { get; }
    }
}