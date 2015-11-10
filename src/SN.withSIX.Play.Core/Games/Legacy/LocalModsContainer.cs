// <copyright company="SIX Networks GmbH" file="LocalModsContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract(Name = "LocalMods", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")
    ]
    public class LocalModsContainer : LocalContainerBase<IContent>, IDisposable, IEnableLogging
    {
        bool _disposed;
        FileSystemWatcher _fsw;
        public LocalModsContainer() {}
        public LocalModsContainer(string name, string path, Game game) : base(name, path, game) {}

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void RefreshLocalLibrary() {
            Items.Clear();
            var path = Path;
            if (path == null || !Directory.Exists(path))
                return;
            TryRefreshLocalLibrary(path);
        }

        public override void FillLocalLibrary(string path) {
            Path = path;
            FillLocalLibrary();
        }

        public override void FillLocalLibrary() {
            DisposeFsw();
            RefreshLocalLibrary();
            SetupFolderChangeTracking();
        }

        void TryRefreshLocalLibrary(string path) {
            try {
                Items.AddRange(CreateLocalMods(path));
            } catch (IOException e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        void SetupFolderChangeTracking() {
            var path = Path;
            if (path == null || !Directory.Exists(path))
                return;

            _fsw = new FileSystemWatcher((path.EndsWith(@"\") ? path : (path + "\\"))) {
                NotifyFilter = NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            _fsw.Changed += OnChanged;
            _fsw.Created += OnChanged;
            _fsw.Deleted += OnChanged;
            _fsw.Renamed += OnRenamed;

            _fsw.EnableRaisingEvents = true;
        }

        ~LocalModsContainer() {
            Dispose(false);
        }

        IEnumerable<IMod> CreateLocalMods(string path) {
            var modding = Game.Modding();
            try {
                return Mod.GetLocalMods(path, Game)
                    .Select(x => CreateLocalMod(x, modding));
            } catch (UnauthorizedAccessException ex) {
                MainLog.Logger.WarnException("Unable to read mods from: " + path, ex);
                return Enumerable.Empty<IMod>();
            }
        }

        IMod CreateLocalMod(LocalModInfo lm, ISupportModding game) {
            return Game.Lists.Mods.FirstOrDefault(y => y.Name.Equals(lm.Name, StringComparison.OrdinalIgnoreCase))
                   ?? Game.Lists.Mods.FirstOrDefault(y => y.Aliases.Contains(lm.Name, StringComparer.OrdinalIgnoreCase))
                   ?? LocalMod.FromLocalModInfo(lm, game);
        }

        void OnChanged(object sender, FileSystemEventArgs args) {
            if (args.FullPath.ToLower().Contains(Repository.DefaultRepoRootDirectory))
                return;
            RefreshLocalLibrary();
        }

        void OnRenamed(object sender, RenamedEventArgs args) {
            if (args.FullPath.ToLower().Contains(Repository.DefaultRepoRootDirectory))
                return;
            RefreshLocalLibrary();
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing) {
                // dispose managed resources
                DisposeFsw();
            }

            // free native resources
            // set large fields to null.
            // call Dispose on your base class if needed
            // base.Dispose(disposing);

            _disposed = true;
        }

        void DisposeFsw() {
            if (_fsw == null)
                return;
            _fsw.EnableRaisingEvents = false;
            _fsw.Dispose();
            _fsw = null;
        }
    }
}