// <copyright company="SIX Networks GmbH" file="SynqConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core;

namespace SN.withSIX.Sync.Presentation.Console
{
    public class SynqConfig
    {
        public string RegisterKey { get; set; }

        public static SynqConfig Load() {
            var file =
                PathConfiguration.GetRoamingRootPath()
                    .GetChildDirectoryWithName("Synq")
                    .GetChildFileWithName("config.json");
            file.MakeSureParentPathExists();
            return Tools.Serialization.Json.LoadJsonFromFile<SynqConfig>(file);
        }
    }
}