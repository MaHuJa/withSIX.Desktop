// <copyright company="SIX Networks GmbH" file="SixSyncModInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Sync.Core.Legacy.SixSync;
using SN.withSIX.Sync.Core.Legacy.Status;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public class SixSyncModInstaller
    {
        readonly IMod _mod;
        readonly ModState _modState;
        IAbsoluteDirectoryPath _path;

        public SixSyncModInstaller(IMod mod, ModState state) {
            _mod = mod;
            _modState = state;
        }

        public async Task ConvertOrInstallOrUpdateInternal(IAbsoluteDirectoryPath path, bool force,
            StatusRepo statusRepo, ModState modState, IAbsoluteDirectoryPath packPath) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(statusRepo != null);
            Contract.Requires<ArgumentNullException>(modState != null);

            _path = path;

            var opts = GetOptions(statusRepo, packPath.ToString(), force);
            if (!modState.Exists) {
                await Install(opts).ConfigureAwait(false);
                return;
            }

            var updateAvailable = modState.State != ContentState.Uptodate;
            if (modState.DoesRepoExist())
                await Update(opts, null, !updateAvailable).ConfigureAwait(false);
            else {
                var repo = Convert(opts);
                await Update(opts, repo).ConfigureAwait(false);
            }
        }

        Uri[] GetHosts() {
            var remotePath = _mod.GetRemotePath();
            return _mod.Mirrors.Select(x => Tools.Transfer.JoinUri(x, remotePath)).ToArray();
        }

        Dictionary<string, object> GetOptions(StatusRepo status, string packPath = null,
            bool allowFallback = false) {
            var opts = new Dictionary<string, object> {
                {"hosts", GetHosts()},
                {"max_threads", GetMaxThreads()},
                {"keep_compressed_files", DomainEvilGlobal.Settings.AppOptions.KeepCompressedFiles},
                {"protocol_preference", DomainEvilGlobal.Settings.AppOptions.ProtocolPreference},
                {"required_version", (long?) _mod.Version.TryIntNullable()},
                {"required_guid", _mod.Guid},
                {"allow_full_transfer_fallback", allowFallback},
                {"output", "none"}
            };

            if (status != null)
                opts.Add("status", status);

            if (!String.IsNullOrWhiteSpace(packPath)) {
                opts["pack_path"] = Repository.RepoTools.GetNewPackPath(packPath,
                    _path.DirectoryName, _modState.Guid);
            }

            return opts;
        }

        int GetMaxThreads() {
            var maxThreads = _mod.GetMaxThreads();
            return maxThreads > 0 ? maxThreads : AppOptions.DefaultPMax; // Custom repos default limit
        }

        Repository Convert(Dictionary<string, object> options) {
            return Repository.Factory.Convert(_path.ToString(), options);
        }

        Task<Repository> Install(Dictionary<string, object> options) {
            return Repository.Factory.Clone((Uri[]) options["hosts"], _path.ToString(), options);
        }

        async Task Update(Dictionary<string, object> options, Repository repo = null,
            bool localOnly = false) {
            options["local_only"] = localOnly;
            if (repo == null)
                repo = Repository.Factory.Open(_path.ToString(), options);
            await repo.Update(options).ConfigureAwait(false);
        }
    }
}