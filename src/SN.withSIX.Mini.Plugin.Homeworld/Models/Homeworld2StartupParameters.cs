// <copyright company="SIX Networks GmbH" file="Homeworld2StartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Homeworld.Models
{
    public class Homeworld2StartupParameters : BasicGameStartupParameters
    {
        public Homeworld2StartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Graphics)]
        [Description("Width of screen in pixels")]
        [DisplayName("Screen Width")]
        public string w
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Graphics)]
        [Description("Height of screen in pixels")]
        [DisplayName("Screen Height")]
        public string h
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        public bool OverrideBigFile
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool LuaTrace
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Graphics)]
        public bool Windowed
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool QuickLoad
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool FreeMouse
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoInt3
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSBW
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSBoth
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSTGA
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSJPG
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSNoLogo
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SuperTurbo
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoS3TC
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoDisplayLists
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoPBuffer
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoSound
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoRender
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadNoROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadPreferROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadMostRecent
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadAlwaysROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool HardwareCursor
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoVideoErrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoMovies
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool silentErrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
    }
}