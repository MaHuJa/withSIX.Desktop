// <copyright company="SIX Networks GmbH" file="ContentEngineGameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.ContentEngine.Infra.Services;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;

namespace SN.withSIX.Play.Infra.Data.Services
{
    public class ContentEngineGameContext : IContentEngineGameContext, IInfrastructureService
    {
        readonly IGameContext _gameContext;

        public ContentEngineGameContext(IGameContext gameContext) {
            _gameContext = gameContext;
        }

        public IContentEngineGame Get(Guid gameId) {
            return _gameContext.Games.Find(gameId);
        }
    }
}