// <copyright company="SIX Networks GmbH" file="MissionUploadViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class MissionUploadViewModel : OverlayViewModelBase
    {
        const string AllowedNameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_ ";
        readonly IContentManager _missionList;
        MissionBase _content;
        List<string> _existingMissions;
        bool _isValid;
        string _missionName;
        double _progress;
        bool _updateMission;
        bool _uploading;
        bool _uploadNewMission;

        public MissionUploadViewModel(IContentManager missionList) {
            DisplayName = "Mission Upload";
            _missionList = missionList;

            this.SetCommand(x => x.SubmitCommand).RegisterAsyncTask(Submit).Subscribe();

            this.WhenAnyValue(x => x.MissionName, x => x.UploadNewMission, x => x.UpdateMission, GetValidity)
                .Subscribe(x => IsValid = x);
        }

        public new bool IsValid
        {
            get { return _isValid; }
            set { SetProperty(ref _isValid, value); }
        }
        public MissionBase Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }
        public bool UploadNewMission
        {
            get { return _uploadNewMission; }
            set { SetProperty(ref _uploadNewMission, value); }
        }
        public bool UpdateMission
        {
            get { return _updateMission; }
            set { SetProperty(ref _updateMission, value); }
        }
        public List<string> ExistingMissions
        {
            get { return _existingMissions; }
            set { SetProperty(ref _existingMissions, value); }
        }
        public string MissionName
        {
            get { return _missionName; }
            set { SetProperty(ref _missionName, value); }
        }
        public bool Uploading
        {
            get { return _uploading; }
            set { SetProperty(ref _uploading, value); }
        }
        public ReactiveCommand SubmitCommand { get; private set; }

        static bool GetValidity(string missionName, bool uploadNew, bool uploadExisting) {
            return (uploadNew || uploadExisting) && ValidateMissionName(missionName);
        }

        static bool ValidateMissionName(string missionName) {
            return !string.IsNullOrWhiteSpace(missionName)
                   && missionName.Length >= 3 && missionName.Length <= 50
                   && missionName.ToLower().All(x => AllowedNameCharacters.Contains(x));
        }

        async Task Submit() {
            await TryPublish().ConfigureAwait(false);
            TryClose(true);
        }

        async Task TryPublish() {
            Uploading = true;
            try {
                await _missionList.PublishMission(Content, MissionName).ConfigureAwait(false);
            } finally {
                Uploading = false;
            }
        }
    }
}