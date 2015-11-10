// <copyright company="SIX Networks GmbH" file="ContentEngineService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.ContentEngine.Infra.Services
{
    public interface IContentEngineService {}

    public abstract class ContentEngineService : IContentEngineService
    {
        protected internal ContentEngineService(RegisteredMod mod) {
            Mod = mod;
        }

        protected RegisteredMod Mod { get; }
    }
}