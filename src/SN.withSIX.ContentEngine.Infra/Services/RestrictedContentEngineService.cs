// <copyright company="SIX Networks GmbH" file="RestrictedContentEngineService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.ContentEngine.Infra.Services
{
    public interface IRestrictedContentEngineService : IContentEngineService {}

    public abstract class RestrictedContentEngineService : ContentEngineService, IRestrictedContentEngineService
    {
        protected RestrictedContentEngineService(RegisteredMod mod) : base(mod) {}
    }
}