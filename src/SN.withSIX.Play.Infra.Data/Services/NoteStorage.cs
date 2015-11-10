// <copyright company="SIX Networks GmbH" file="NoteStorage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Text;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Infrastructure;

namespace SN.withSIX.Play.Infra.Data.Services
{
    [Obsolete("Convert to Akavache")]
    public class NoteStorage : INoteStorage, IInfrastructureService
    {
        public string GetNotes(Server server) {
            return GetNotes(server, "Server");
        }

        public string GetNotes(MissionBase mission) {
            return GetNotes(mission, "Mission");
        }

        public string GetNotes(Collection collection) {
            return GetNotes(collection, "Collection");
        }

        public void SetNotes(Server server, string text) {
            SetNotes(server, "Server", text);
        }

        public void SetNotes(MissionBase mission, string text) {
            SetNotes(mission, "Server", text);
        }

        public void SetNotes(Collection collection, string text) {
            SetNotes(collection, "Collection", text);
        }

        public bool HasNotes(Server server) {
            return HasNotes(server, "Server");
        }

        public bool HasNotes(MissionBase mission) {
            return HasNotes(mission, "Mission");
        }

        public bool HasNotes(Collection collection) {
            return HasNotes(collection, "Collection");
        }

        string GetNotes(IHaveNotes note, string type) {
            var fileName = GetNoteFileName(note, type);
            return fileName.Exists ? File.ReadAllText(fileName.ToString(), Encoding.UTF8) : String.Empty;
        }

        void SetNotes(IHaveNotes note, string type, string text) {
            var fileName = GetNoteFileName(note, type);
            if (string.IsNullOrWhiteSpace(text)) {
                if (fileName.Exists)
                    Tools.FileUtil.Ops.DeleteWithRetry(fileName.ToString());
            } else {
                var noteDir = GetNoteDirectory(type);
                noteDir.MakeSurePathExists();
                File.WriteAllText(fileName.ToString(), text, Encoding.UTF8);
            }
        }

        bool HasNotes(IHaveNotes note, string type) {
            return GetNoteFileName(note, type).Exists;
        }

        IAbsoluteDirectoryPath GetNoteDirectory(string type) {
            return Common.Paths.NotePath.GetChildDirectoryWithName(type + "s");
        }

        IAbsoluteFilePath GetNoteFileName(IHaveNotes noter, string type) {
            return
                GetNoteDirectory(type)
                    .GetChildFileWithName(string.Format(type + "_{0}.txt",
                        noter.NoteName.Replace(".", "_").Replace(":", "_")));
        }
    }
}