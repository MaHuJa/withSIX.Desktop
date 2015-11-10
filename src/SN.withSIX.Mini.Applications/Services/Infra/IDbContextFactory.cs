// <copyright company="SIX Networks GmbH" file="IDbContextFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface IDbContextFactory
    {
        IDbContextScope Create();
    }

    public interface IDbContextLocator
    {
        IGameContext GetGameContext();
        IGameContextReadOnly GetReadOnlyGameContext();
        ISettingsStorage GetSettingsContext();
        ISettingsStorageReadOnly GetReadOnlySettingsContext();
    }

    public interface IDbContextScope : IDisposable {}
}