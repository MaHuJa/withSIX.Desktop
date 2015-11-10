// <copyright company="SIX Networks GmbH" file="AceUserconfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Mini.Plugin.Arma.Models.Legacy
{
    [Obsolete("Convert to ContentEngine")]
    public abstract class Userconfig : PropertyChangedBase
    {
        [IgnoreDataMember]
        [Browsable(false)]
        public string Name => GetType().Name;
        public abstract bool Save();
    }

    [Obsolete("Convert to ContentEngine")]
    public class AceUserconfig : Userconfig, IEnableLogging
    {
        static readonly char[] _trimchar = {' ', '\t'};
        readonly Dictionary<string, List<object>> _acedefines;
        readonly ArmaGame _game;
        bool _balltracers;
        string _customface;
        bool _disableruckless;
        bool _grenadethrow;
        string[] _identities;
        bool _minimalhud;
        bool _newcompass;
        bool _nocross;
        bool _nodiarystats;
        bool _novoice;
        bool _reticles;

        public AceUserconfig(ArmaGame game) {
            Identities = game.GetProfiles().ToArray();

            _game = game;
            _acedefines = new Dictionary<string, List<object>> {
                // List<object> = value, line number, internal private variable name as string to get and set it via reflection
                {"ACE_IDENTITY", new List<object> {"", -1, "Identity"}},
                {"ACE_CUSTOMFACE", new List<object> {"", -1, "CustomFace"}},
                {"ACE_COMPASS", new List<object> {false, -1, "NewCompass"}},
                {"ACE_RETICLES", new List<object> {false, -1, "Reticles"}},
                {"ACE_NOVOICE", new List<object> {false, -1, "NoVoice"}},
                {"ACE_NOCROSS", new List<object> {false, -1, "NoCross"}},
                {"ACE_MINIMALHUD", new List<object> {false, -1, "MinimalHud"}},
                {"ACE_DISABLE_RUCKLESS", new List<object> {false, -1, "DisableRuckless"}},
                {"ACE_BALLTRACERS", new List<object> {false, -1, "BallTracers"}},
                {"ACE_GRENADETHROW", new List<object> {false, -1, "GrenadeThrow"}},
                {"ACE_NODIARYSTATISTICS", new List<object> {false, -1, "NoDiaryStats"}}
            };
        }

        [Description("Select an identity")]
        // TODO
        //[ItemsSource(typeof (ArmaStartupParams.PlayerProfileItemsSource))]
        public string Identity { get; set; }
        [Browsable(false)]
        public string[] Identities
        {
            get { return _identities; }
            set { SetProperty(ref _identities, value); }
        }
        [Description("Select a custom face")]
        public string CustomFace
        {
            get { return _customface; }
            set { SetProperty(ref _customface, value); }
        }
        [Description("Disables AI radio chatter as well as player radio chatter")]
        public bool NoVoice
        {
            get { return _novoice; }
            set { SetProperty(ref _novoice, value); }
        }
        [Description("Disables/Reduces crosshairs")]
        public bool NoCross
        {
            get { return _nocross; }
            set { SetProperty(ref _nocross, value); }
        }
        [Description("More Screen information, less HUD stuff")]
        public bool MinimalHud
        {
            get { return _minimalhud; }
            set { SetProperty(ref _minimalhud, value); }
        }
        [Description("Ruckless disables models with built-in Rucksacks on the models")]
        public bool DisableRuckless
        {
            get { return _disableruckless; }
            set { SetProperty(ref _disableruckless, value); }
        }
        [Description("Alternative round tracers")]
        public bool BallTracers
        {
            get { return _balltracers; }
            set { SetProperty(ref _balltracers, value); }
        }
        [Description("Enable alternative new compass")]
        public bool NewCompass
        {
            get { return _newcompass; }
            set { SetProperty(ref _newcompass, value); }
        }
        [Description("Enable animated reticles")]
        public bool Reticles
        {
            get { return _reticles; }
            set { SetProperty(ref _reticles, value); }
        }
        [Description("Enable scripted grenade throwing")]
        public bool GrenadeThrow
        {
            get { return _grenadethrow; }
            set { SetProperty(ref _grenadethrow, value); }
        }
        [Description("Disable Diary Statistics")]
        public bool NoDiaryStats
        {
            get { return _nodiarystats; }
            set { SetProperty(ref _nodiarystats, value); }
        }

        public bool ReadAceClientSideConfig() {
            var gamePath = _game.InstalledState.Directory;
            if (gamePath == null)
                return false;
            var file = gamePath + @"\userconfig\ace\ace_clientside_config.hpp";
            if (!File.Exists(file))
                return false;

            Identity = "";
            CustomFace = "";
            NewCompass = false;
            Reticles = false;
            NoVoice = false;
            NoCross = false;
            MinimalHud = false;
            DisableRuckless = false;
            BallTracers = false;
            GrenadeThrow = false;
            NoDiaryStats = false;

            return TryReadFromFile(file);
        }

        bool TryReadFromFile(string file) {
            try {
                using (var sr = new StreamReader(file, Encoding.UTF8)) {
                    string line;
                    var curlinenr = 0;
                    while ((line = sr.ReadLine()) != null) {
                        CheckLineClientConfig(line, _trimchar, curlinenr);
                        curlinenr++;
                    }
                    sr.Close();
                }
                return true;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                return false;
            }
        }

        void CheckLineClientConfig(string line, char[] trimchar, int curlinenr) {
            line = line.Trim(trimchar);
            if (line.Length == 0 || line.StartsWith("//"))
                return;

            const string cdefine = "#define";

            foreach (var kvp in _acedefines) {
                if ((int) kvp.Value[1] != -1)
                    continue;

                var _nindex = line.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase);
                if (_nindex == -1)
                    continue;

                var fi = typeof (AceUserconfig).GetProperty((string) kvp.Value[2],
                    BindingFlags.Public | BindingFlags.Instance);

                if (kvp.Value[0] is bool)
                    fi.SetValue(this, true, null);
                else {
                    if (kvp.Value[0] is string) {
                        var _hindex = line.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase);
                        if (_hindex != -1)
                            line = line.Remove(_hindex, kvp.Key.Length);
                        _hindex = line.IndexOf(cdefine, StringComparison.OrdinalIgnoreCase);
                        if (_hindex != -1)
                            line = line.Remove(_hindex, cdefine.Length);
                        fi.SetValue(this, line.Trim(), null);
                    }
                }
                kvp.Value[1] = curlinenr;
                return;
            }
        }

        public override bool Save() {
            return WriteClientSideConfig(_game.InstalledState.Directory);
        }

        bool WriteClientSideConfig(IAbsoluteDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(path != null);

            var acePath = Path.Combine(path.ToString(), @"userconfig\ace");
            var file = Path.Combine(acePath, "ace_clientside_config.hpp");

            if (!Directory.Exists(acePath)) {
                if (!TryCreatePath(acePath))
                    return false;
            }

            var file2 = Path.Combine(acePath, "ace_serverconfig.hpp");
            if (!File.Exists(file2))
                TryWriteNewConfig(file2);

            return TryWriteConfig(file);
        }

        void TryWriteNewConfig(string file2) {
            try {
                using (
                    var tw =
                        new StreamWriter(
                            file2,
                            false, Encoding.UTF8)) {
                    tw.WriteLine("class ace_server_settings {");
                    tw.WriteLine("check_pbos = 1;");
                    tw.WriteLine("check_all_ace_pbos = 1;");
                    tw.WriteLine("checklist[] = {};");
                    tw.WriteLine("exclude_pbos[] = {};");
                    tw.WriteLine("};");
                    tw.WriteLine("// #define VERSIONING_LEVEL -1");
                    tw.WriteLine("ACE_SERVERCONFIG_VER = 2;");
                    tw.WriteLine();
                }
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Failed to write userconfig", null));
            }
        }

        bool TryWriteConfig(string file) {
            try {
                using (var tw = new StreamWriter(file, false, Encoding.UTF8)) {
                    foreach (var kvp in _acedefines) {
                        var _addcomment = string.Empty;
                        var fi = typeof (AceUserconfig).GetProperty((string) kvp.Value[2],
                            BindingFlags.Public | BindingFlags.Instance);
                        if (kvp.Value[0] is bool) {
                            if (!(bool) fi.GetValue(this, null))
                                _addcomment = "//";
                            tw.WriteLine(_addcomment + "#define " + kvp.Key);
                        } else {
                            if (kvp.Value[0] is string) {
                                _addcomment = string.Empty;
                                var _fieldval = (string) fi.GetValue(this, null);
                                if (_fieldval == String.Empty)
                                    _addcomment = "//";
                                tw.WriteLine(_addcomment + "#define " + kvp.Key + " " + _fieldval);
                            }
                        }
                    }
                    tw.WriteLine("ACE_CLIENTSIDE_CONFIG_VER = 14;");
                    tw.Flush();
                    tw.Close();
                }
                return true;
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Failed to write userconfig", null));
                return false;
            }
        }

        bool TryCreatePath(string acePath) {
            try {
                acePath.MakeSurePathExists();
            } catch (Exception e) {
                UserError.Throw(new InformationalUserError(e, "Failed to create userconfig directory", null));
                return false;
            }
            return true;
        }
    }
}