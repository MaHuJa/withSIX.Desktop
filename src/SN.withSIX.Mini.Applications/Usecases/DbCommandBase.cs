// <copyright company="SIX Networks GmbH" file="GameRequestBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public abstract class DbRequestBase
    {
        protected readonly IDbContextLocator DbContextLocator;

        protected DbRequestBase(IDbContextLocator dbContextLocator) {
            DbContextLocator = dbContextLocator;
        }
    }

    public abstract class DbCommandBase : DbRequestBase
    {
        protected DbCommandBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
        protected IGameContext GameContext => DbContextLocator.GetGameContext();
        protected ISettingsStorage SettingsContext => DbContextLocator.GetSettingsContext();
    }

    public abstract class DbQueryBase : DbRequestBase
    {
        protected DbQueryBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
        protected IGameContextReadOnly GameContext => DbContextLocator.GetReadOnlyGameContext();
        protected ISettingsStorageReadOnly SettingsContext => DbContextLocator.GetReadOnlySettingsContext();
    }
}