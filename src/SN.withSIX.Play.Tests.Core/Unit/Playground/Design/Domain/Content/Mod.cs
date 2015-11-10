// <copyright company="SIX Networks GmbH" file="Mod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public abstract class Mod : Content
    {
        protected Mod(Guid id, ModMetaData metaData) : base(id) {
            MetaData = metaData;
        }

        public ModMetaData MetaData { get; private set; }
    }

    public abstract class Mod<TGameData, TLaunchSomething> : Mod, IProcessableContent<TGameData>,
        ILaunchableContent<TLaunchSomething>
        where TGameData : IModdingGameData
    {
        protected Dependency _desiredVersion;

        protected Mod(Guid id, PackageItem package, ModMetaData metaData) : base(id, metaData) {
            Package = package;
        }

        public PackageItem Package { get; private set; }

        public bool IsLocal { get; private set; }
        public abstract void MakeLaunchState(TLaunchSomething sharedLaunchState, IMediator domainMediator);
        public IEnumerable<ILaunchableContent<TLaunchSomething>> Dependencies { get; set; }

        public virtual async Task Install(TGameData gameData, IMediator mediator) {
            if (IsLocal)
                return;
            await mediator.RequestAsync(new InstallSynqPackageCommand(Package, gameData.ModPaths)).ConfigureAwait(false);
            RefreshInstalledState(gameData);
        }

        public virtual async Task Uninstall(TGameData gameData, IMediator mediator) {
            if (IsLocal)
                DeleteAsync();
            else {
                await
                    mediator.RequestAsync(new UninstallSynqPackageCommand(Package, gameData.ModPaths))
                        .ConfigureAwait(false);
            }

            RefreshInstalledState(gameData);
        }

        public virtual async Task Verify(TGameData gameData, IMediator mediator) {
            await
                mediator.RequestAsync(new VerifySynqPackageCommand(Package, gameData.ModPaths)).ConfigureAwait(false);
            RefreshInstalledState(gameData);
        }

        protected abstract void DeleteAsync();

        public void RefreshInstalledState(TGameData gameData) {
            InstalledState = GetInstalledState(gameData);
        }

        protected abstract ContentInstalledState GetInstalledState(TGameData gameData);

        public virtual void SetDesiredVersion(Dependency version) {
            _desiredVersion = version;
        }

        public virtual IReadOnlyCollection<Dependency> GetVersionStates(TGameData gameData) {
            return Package.Packages.ToList().AsReadOnly();
        }
    }
}