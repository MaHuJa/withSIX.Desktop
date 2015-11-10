// <copyright company="SIX Networks GmbH" file="InstallContentAction.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using NDepend.Path;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller.Attributes;

namespace SN.withSIX.Mini.Core.Games
{
    public class DownloadContentAction : ContentAction<IInstallableContent>
    {
        public DownloadContentAction(CancellationToken cancelToken = new CancellationToken(),
            params InstallContentSpec[] content) : base(content, cancelToken) {}
    }

    public class InstallContentAction : ContentAction<IInstallableContent>,
        IInstallContentAction<IInstallableContent>
    {
        public InstallContentAction(IReadOnlyCollection<IContentSpec<IInstallableContent>> content,
            CancellationToken cancelToken = default(CancellationToken)) : base(content, cancelToken) {}

        public Game Game { get; set; }
        public RemoteInfoAttribute RemoteInfo { get; set; }
        public ContentPaths Paths { get; set; }
        public CheckoutType CheckoutType { get; set; } = CheckoutType.NormalCheckout;
        public InstallStatusOverview Status { get; } = CreateInstallStatusOverview();
        public InstallerType InstallerType { get; }
        [Obsolete(
            "Should no longer be needed once we refactor all to install packages to a certain folder, and then use symlinks when needed"
            )]
        public IAbsoluteDirectoryPath GlobalWorkingPath { get; set; }
        public ContentCleaningAttribute Cleaning { get; set; } = ContentCleaningAttribute.Default;

        public static InstallStatusOverview CreateInstallStatusOverview() {
            return new InstallStatusOverview {
                Missions = CreateInstallStatus(),
                Mods = CreateInstallStatus(),
                Collections = CreateInstallStatus()
            };
        }

        static InstallStatus CreateInstallStatus() {
            return new InstallStatus {
                Install = new List<Guid>(),
                Uninstall = new List<Guid>(),
                Update = new List<Guid>()
            };
        }
    }

    public interface IInstallContentAction<out T> : IContentAction<T> where T : IContent
    {
        InstallStatusOverview Status { get; }
        Game Game { get; }
        RemoteInfoAttribute RemoteInfo { get; }
        ContentPaths Paths { get; }
        CheckoutType CheckoutType { get; }
        InstallerType InstallerType { get; }
        IAbsoluteDirectoryPath GlobalWorkingPath { get; }
        ContentCleaningAttribute Cleaning { get; }
    }

    public class UnInstallContentAction : ContentAction<IUninstallableContent>,
        IUninstallContentAction2<IUninstallableContent>
    {
        public UnInstallContentAction(Game game, IReadOnlyCollection<IContentSpec<IUninstallableContent>> content,
            CancellationToken cancelToken = default(CancellationToken)) : base(content, cancelToken) {
            Game = game;
        }

        public InstallStatusOverview Status { get; } = InstallContentAction.CreateInstallStatusOverview();
        public Game Game { get; }
        public ContentPaths Paths { get; set; }
    }

    public interface IUninstallContentAction2<out T> : IContentAction<T> where T : IContent
    {
        Game Game { get; }
        ContentPaths Paths { get; }
        InstallStatusOverview Status { get; }
    }

    [Obsolete("Temporary until we refactor to install packages to folders and use symlinks/CE instead")]
    public enum CheckoutType
    {
        NormalCheckout,
        CheckoutWithoutRemoval
    }

    public enum InstallerType
    {
        Synq
    }
}