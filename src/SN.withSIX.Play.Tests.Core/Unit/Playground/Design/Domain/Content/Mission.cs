// <copyright company="SIX Networks GmbH" file="Mission.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Commands;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public abstract class Mission : Content
    {
        public Mission(Guid id, MissionMetaData metaData) : base(id) {
            MetaData = metaData;
        }

        public MissionMetaData MetaData { get; private set; }
    }

    public abstract class Mission<TGameData, TLaunchSomething> : Mission, IProcessableContent<TGameData>,
        ILaunchableContent<TLaunchSomething> where TGameData : IMissionGameData
    {
        protected Mission(Guid id, MissionMetaData metaData, PackageItem package) : base(id, metaData) {
            Package = package;
        }

        public PackageItem Package { get; private set; }
        public abstract IEnumerable<ILaunchableContent<TLaunchSomething>> Dependencies { get; }
        public abstract void MakeLaunchState(TLaunchSomething sharedLaunchState, IMediator domainMediator);

        public virtual Task Install(TGameData gameData, IMediator mediator) {
            return
                mediator.RequestAsync(new InstallFileBasedSynqPackageCommand(Package,
                    gameData.MissionPaths[0]));
        }

        public virtual Task Uninstall(TGameData gameData, IMediator mediator) {
            return
                mediator.RequestAsync(new UninstallFileBasedSynqPackageCommand(Package, gameData.MissionPaths[0]));
        }

        public virtual Task Verify(TGameData gameData, IMediator mediator) {
            return
                mediator.RequestAsync(new VerifyFileBasedSynqPackageCommand(Package, gameData.MissionPaths[0]));
        }
    }
}