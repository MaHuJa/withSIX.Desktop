// <copyright company="SIX Networks GmbH" file="IMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using NDepend.Path;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    // We cannot remove this yet due to ToggleableModProxy/IToggleAbleContent
    // Unless we make most virtual. But better would be to replace it at some point
    [Obsolete("Should be disbanded once ToggleableMod proxy is handled differently")]
    public interface IMod : IContent, IHaveType<GameModType>, IComparePK<IMod>, IContentEngineContent
    {
        Guid Id { get; }
        Guid NetworkId { get; set; }
        [Obsolete("Should be in a view/data model")]
        bool IsInCurrentCollection { get; set; }
        string DisplayName { get; }
        string Revision { get; set; }
        ModController Controller { get; }
        DateTime UpdatedVersion { get; set; }
        IAbsoluteDirectoryPath CustomPath { get; set; }
        string CppName { get; set; }
        string FullName { get; set; }
        string MinBuild { get; set; }
        string MaxBuild { get; set; }
        Guid GameId { get; set; }
        /// <summary>
        ///     Only used for SixSync custom repositories
        /// </summary>
        string Guid { get; set; }
        bool HasLicense { get; set; }
        string ModVersion { get; set; }
        long Size { get; set; }
        long SizeWd { get; set; }
        string[] Aliases { get; set; }
        string[] Dependencies { get; set; }
        Uri[] Mirrors { get; set; }
        List<Network> Networks { get; set; }
        Userconfig UserConfig { get; }
        string UserConfigChecksum { get; set; }
        int GetMaxThreads();
        string GetSerializationString();
        Guid[] GetGameRequirements();
        void OpenChangelog();
        string GetUrl(string type = "changelog");
        void OpenReadme();
        void OpenLicense();
        void OpenHomepage();
        bool GameMatch(ISupportModding game);
        bool RequiresAdminRights();
        IEnumerable<IAbsolutePath> GetPaths();
        void LoadSettings(ISupportModding game);
        bool Match(string name);
        string GetRemotePath();
        bool CompatibleWith(ISupportModding game);
    }
}