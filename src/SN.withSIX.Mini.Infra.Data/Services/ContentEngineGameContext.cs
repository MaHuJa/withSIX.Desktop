// <copyright company="SIX Networks GmbH" file="ContentEngineGameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.ContentEngine.Infra.Services;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    public class ContentEngineGameContext : IContentEngineGameContext, IInfrastructureService
    {
        readonly IDbContextLocator _contextLocator;

        public ContentEngineGameContext(IDbContextLocator contextLocator) {
            _contextLocator = contextLocator;
        }

        public IContentEngineGame Get(Guid gameId) {
            return _contextLocator.GetReadOnlyGameContext().FindGame(gameId);
        }
    }
}