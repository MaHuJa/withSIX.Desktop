// <copyright company="SIX Networks GmbH" file="SaveSelectedGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class SaveSelectedGame : IAsyncVoidCommand, IExcludeWriteLock // TODO: WriteLock for Db vs Settings?!
    {
        public SaveSelectedGame(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class SaveSelectedGameHandler : DbCommandBase, IAsyncVoidCommandHandler<SaveSelectedGame>
    {
        public SaveSelectedGameHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public Task<UnitType> HandleAsync(SaveSelectedGame request) {
            SettingsContext.Settings.Local.SelectedGameId = request.Id;
            return SettingsContext.SaveSettings().Void();
        }
    }
}