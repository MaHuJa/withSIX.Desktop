// <copyright company="SIX Networks GmbH" file="GameRequestBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public abstract class DbRequestBase
    {
        protected readonly IDbContextLocator DbContextLocator;

        protected DbRequestBase(IDbContextLocator dbContextLocator)
        {
            DbContextLocator = dbContextLocator;
        }
    }

    public abstract class DbCommandBase : DbRequestBase
    {
        protected IGameContext GameContext => DbContextLocator.GetGameContext();
        protected ISettingsStorage SettingsContext => DbContextLocator.GetSettingsContext();
        protected DbCommandBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
    }

    public abstract class DbQueryBase : DbRequestBase
    {
        protected IGameContextReadOnly GameContext => DbContextLocator.GetReadOnlyGameContext();
        protected ISettingsStorageReadOnly SettingsContext => DbContextLocator.GetReadOnlySettingsContext();
        protected DbQueryBase(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}
    }
}