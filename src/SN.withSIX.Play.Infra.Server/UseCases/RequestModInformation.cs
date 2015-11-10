// <copyright company="SIX Networks GmbH" file="RequestModInformation.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Infra.Server.UseCases
{
    public class RequestModInformation : IRequestModInformationBase, IAsyncRequest<IModInfo>
    {
        public RequestModInformation(Guid modID) {
            ModID = modID;
            GameID = Guid.Empty;
        }

        public RequestModInformation(Guid modID, Guid gameID) {
            ModID = modID;
            GameID = gameID;
        }

        public Guid? ModID { get; }
        public Guid GameID { get; }
    }

    public interface IRequestModInformationBase
    {
        Guid GameID { get; }
    }

    public class RequestModInformationByGame : IAsyncRequest<Dictionary<Guid, IModInfo>>, IRequestModInformationBase
    {
        public RequestModInformationByGame(Guid gameID) {
            GameID = gameID;
            OnlyInstalled = true;
        }

        public RequestModInformationByGame(Guid gameID, bool onlyInstalled) {
            GameID = gameID;
            OnlyInstalled = onlyInstalled;
        }

        public bool OnlyInstalled { get; }
        public Guid GameID { get; }
    }

    public class RequestModInformationByGameHandler : BaseHandler,
        IAsyncRequestHandler<RequestModInformationByGame, Dictionary<Guid, IModInfo>>
    {
        public RequestModInformationByGameHandler(IGameContext gameContext) : base(gameContext) {}

        public Task<Dictionary<Guid, IModInfo>> HandleAsync(RequestModInformationByGame request) {
            var mods = GetMods(request);
            return Task.FromResult(request.OnlyInstalled
                ? mods.Where(x => x.State != ContentState.NotInstalled)
                    .Select(GetModInfo)
                    .ToDictionary(result => result.Id)
                : mods.Select(x => (IModInfo) new ModInfo(x)).ToDictionary(result => result.Id));
        }

        static IModInfo GetModInfo(IMod x) {
            return new ModInfo(x);
        }
    }

    public class RequestModInformationHandler : BaseHandler, IAsyncRequestHandler<RequestModInformation, IModInfo>
    {
        public RequestModInformationHandler(IGameContext gameContext) : base(gameContext) {}

        public async Task<IModInfo> HandleAsync(RequestModInformation request) {
            var mods = GetMods(request);
            var mod = mods.First(x => x.Guid == request.ModID.ToString());

            return new ModInfo(mod);
        }
    }

    public abstract class BaseHandler
    {
        readonly IGameContext _gameContext;

        protected BaseHandler(IGameContext gameContext) {
            _gameContext = gameContext;
        }

        protected IQueryable<IMod> GetMods(IRequestModInformationBase request) {
            return request.GameID != null
                ? _gameContext.Games.Where(x => x.Id == request.GameID && x.SupportsMods())
                    .SelectMany(x => x.Lists.Mods)
                : _gameContext.Games.Where(x => x.SupportsMods()).SelectMany(x => x.Lists.Mods);
        }
    }
}