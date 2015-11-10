// <copyright company="SIX Networks GmbH" file="GetContentEngineService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    // TODO: Infra should not contain use cases. It's only here because CE is using Mediator to construct services: Not what it is designed for!
    public interface IGetContentEngineService<TResponseData> : IRequest<TResponseData>
    {
        RegisteredMod Mod { get; }
    }

    public abstract class GetContentEngineService<TResponseData> : IGetContentEngineService<TResponseData>
    {
        protected GetContentEngineService(RegisteredMod mod) {
            Mod = mod;
        }

        public RegisteredMod Mod { get; }
    }
}