// <copyright company="SIX Networks GmbH" file="GameDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class GameDataModel : ContentDataModel
    {
        string _directory;
        DlcDataModel[] _dlcs;
        string _executable;
        bool _isInstalled;
        RunningGame _running;
        GameSettingsDataModel _settings;
        string _shortName;
        string _startupLine;
        Version _version;
        public GameDataModel(Guid id) : base(id) {}
        public string ShortName
        {
            get { return _shortName; }
            set { SetProperty(ref _shortName, value); }
        }
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }
        public string StartupLine
        {
            get { return _startupLine; }
            set { SetProperty(ref _startupLine, value); }
        }
        public Version Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }
        public DlcDataModel[] Dlcs
        {
            get { return _dlcs; }
            set { SetProperty(ref _dlcs, value); }
        }
        public string Directory
        {
            get { return _directory; }
            set { SetProperty(ref _directory, value); }
        }
        public string Executable
        {
            get { return _executable; }
            set { SetProperty(ref _executable, value); }
        }
        public RunningGame Running
        {
            get { return _running; }
            set { SetProperty(ref _running, value); }
        }
        public bool SupportsMods { get; set; }
        public bool SupportsServers { get; set; }
        public bool SupportsMissions { get; set; }
        public GameSettingsDataModel Settings
        {
            get { return _settings; }
            set { SetProperty(ref _settings, value); }
        }
    }
}