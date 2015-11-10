// <copyright company="SIX Networks GmbH" file="ModNetworkContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public class ModNetworkContent : NetworkContent, IModContent
    {
        protected ModNetworkContent() {}
        public ModNetworkContent(string name, string packageName, Guid gameId) : base(name, packageName, gameId) {}

        public override string ContentSlug { get; } = "mods";

        public override async Task PostInstall(IInstallerSession installerSession, CancellationToken cancelToken) {
            await base.PostInstall(installerSession, cancelToken).ConfigureAwait(false);
            installerSession.RunCE(this); // TODO: run CE on custom repo content, and somehow figure out the NetworkId vs RepoId like in PwS..
        }
    }
}