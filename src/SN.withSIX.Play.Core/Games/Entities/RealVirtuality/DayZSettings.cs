// <copyright company="SIX Networks GmbH" file="DayZSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class DayZSettings : RealVirtualitySettings
    {
        public DayZSettings(Guid gameId, DayZStartupParams sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            Filter = new ArmaServerFilter();
        }

        public ArmaServerFilter Filter { get; set; }
    }

    public class DayZStartupParams : RealVirtualityStartupParameters
    {
        public DayZStartupParams(params string[] defaultParameters) : base(defaultParameters) {}
        [Description("Additional modline"), Category(GameSettingCategories.Basic)]
        public string Mod
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer)]
        public string Init
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description("Alternative to mod parameter, to use for Beta patches")]
        public string Beta
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Defines memory allocation limit to number (in MegaBytes).\n256 is hard-coded minimum (anything lower falls backs to 256). 2047 is hard-coded maximum (anything higher falls back to 2047).\nEngine uses automatic values (512-1536 MB) w/o maxMem parameter."
            )]
        public string MaxMem
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Defines Video Memory allocation limit to number (in MegaBytes).\n128 is hard-coded minimum (anything lower falls backs to 128). 2047 is soft-coded maximum , any value over 2GB might result into unforseen consequences!"
            )]
        public string MaxVram
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Change to a number less or equal than numbers of available cores. This will override auto detection")]
        public string CpuCount
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value == null || value.TryInt() < 0 ? null : value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Level of multi-treading. Higher means more multi-threading\nChange to a number 0,1,3,5,7. This will override auto detection (which use 3 for dualcore and 7 for quadcore)."
            )]
        //[ItemsSource(typeof (ExThreadsItemsSource))] // TODO
        public string ExThreads
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Set the particular allocator to be used. Significantly affects both performance and stability of Game"
            )
        ]
        public string Malloc
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Server), Description("Server IP to connect to")]
        public string Connect
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Server), Description("Server port to connect to")]
        public string Port
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Server), Description("Server password to connect with")]
        public string Password
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Locations), Description("PID file location")]
        public string Pid
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Server), Description("Basic configuration file")]
        public string Cfg
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Locations), Description("BattlEye location")]
        public string BePath
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Locations), Description("PAR file location")]
        public string Par
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Basic), Description(
            "Displays Arma windowed instead of full screen. Screen resolution / window size are set in arma2.cfg")]
        public bool Window
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Compatibility), Description(
            "Forces Game to use Direct3D version 9 only, not the extended Vista / Win7 Direct3D 9Ex\nThe most visible feature the Direct3D 9Ex version offers is a lot faster alt-tabing. May help with problems using older drivers on multi-GPU systems."
            )]
        public bool Winxp
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Compatibility),
         Description("Turns off multicore use. It slows down rendering but may resolve visual glitches.")]
        public bool NoCb
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description("Ensures that only PBOs are loaded and NO unpacked data")
        ]
        public bool NoFilePatching
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer),
         Description(
             "Allow Game running even when its window does not have focus (i.e. running in the background)")
        ]
        public bool NoPause
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer), Description("Introduced to show errors in scripts on-screen")]
        public bool ShowScriptErrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Advanced), Description(
            "Introduced to provide thorough test of all signatures of all loaded banks at the start game. Output is in .rpt file"
            )]
        public bool CheckSignatures
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer), Description("Starts with no world loaded. (Used for Buldozer)")]
        public bool NoLand
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer), Description("Disables sound output.")]
        public bool NoSound
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Developer), Description("Engine closes immediately after detecting this option")
        ]
        public bool DoNothing
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
    }
}