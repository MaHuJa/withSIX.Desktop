// <copyright company="SIX Networks GmbH" file="TransferStatus.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Transfer
{
    [DoNotObfuscateType]
    public class TransferStatus : ModelBase, ITransferStatus
    {
        readonly Object _outputLock = new Object();
        readonly ConsoleWriter _text = new ConsoleWriter();
        RepoStatus _action;
        string _color;
        bool _completed;
        TimeSpan? _eta;
        bool _failed;
        long _fileSize;
        long _fileSizeNew;
        long _fileSizeTransfered;
        string _fileStatus;
        string _info;
        string _output;
        string _processCl;
        double _progress;
        long _speed;
        TimeSpan? _timeTaken;

        protected TransferStatus() {
            CreatedAt = Tools.Generic.GetCurrentUtcDateTime;
            UpdatedAt = Tools.Generic.GetCurrentUtcDateTime;
        }

        public TransferStatus(string item, double progress = 0, int speed = 0,
            TimeSpan? eta = null, RepoStatus action = RepoStatus.Waiting) : this() {
            Item = item;
            _progress = progress;
            _speed = speed;
            _eta = eta;
            _action = action;
        }

        public bool ZsyncIncompatible { get; set; }
        public bool ZsyncHttpFallback { get; set; }
        public int ZsyncHttpFallbackAfter { get; set; } = 60;
        public int Tries { get; set; }

        public void EndOutput() {
            UpdateTimeTaken();
            Eta = null;
            Speed = 0;
            Action = RepoStatus.Finished;
            Progress = 100;
            Completed = true;
        }

        public void EndOutput(string f) {
            if (File.Exists(f))
                FileSizeNew = new FileInfo(f).Length;
            EndOutput();
        }

        public void FailOutput() {
            UpdateTimeTaken();
            Eta = null;
            Speed = 0;
            Action = RepoStatus.Failed;
            Completed = true;
            Fail();
        }

        public void FailOutput(string f) {
            if (File.Exists(f))
                FileSizeNew = new FileInfo(f).Length;
            FailOutput();
        }

        public void StartOutput(string f) {
            if (File.Exists(f)) {
                FileSize = new FileInfo(f).Length;
                FileStatus = "Existing";
            } else
                FileStatus = "New";
        }

        #region IStatus Members

        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        public TimeSpan? TimeTaken
        {
            get { return _timeTaken; }
            set { SetProperty(ref _timeTaken, value); }
        }

        public bool Failed
        {
            get { return _failed; }
            set { SetProperty(ref _failed, value); }
        }

        public string Item { get; }

        public string Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }

        public string FileStatus
        {
            get { return _fileStatus; }
            set { SetProperty(ref _fileStatus, value); }
        }

        public RepoStatus Action
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }

        public string ProcessCl
        {
            get { return _processCl; }
            set { SetProperty(ref _processCl, value); }
        }

        public virtual long FileSize
        {
            get { return _fileSize; }
            set { SetProperty(ref _fileSize, value); }
        }

        public virtual long FileSizeNew
        {
            get { return _fileSizeNew; }
            set { SetProperty(ref _fileSizeNew, value); }
        }

        public string ZsyncLoopData { get; set; }
        public int ZsyncLoopCount { get; set; }

        public bool Completed
        {
            get { return _completed; }
            set { SetProperty(ref _completed, value); }
        }

        public TimeSpan? Eta
        {
            get { return _eta; }
            set { SetProperty(ref _eta, value); }
        }

        public string Output
        {
            get { return _output; }
            set { SetProperty(ref _output, value); }
        }

        public long Speed
        {
            get { return _speed; }
            set { SetProperty(ref _speed, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetPropertySafe(ref _progress, value); }
        }

        public virtual long FileSizeTransfered
        {
            get { return _fileSizeTransfered; }
            set { SetProperty(ref _fileSizeTransfered, value); }
        }

        public void UpdateOutput(string data) {
            lock (_outputLock) {
                _text.UpdateOutput(data);
                Output = _text.ToString();
            }
        }

        public void ResetZsyncLoopInfo() {
            ZsyncLoopCount = 0;
            ZsyncLoopData = null;
        }

        public void Reset(RepoStatus action = RepoStatus.Processing) {
            UpdateData(0, 0);
            Action = action;
        }

        public void UpdateStamp() {
            UpdatedAt = Tools.Generic.GetCurrentUtcDateTime;
        }

        public void UpdateTimeTaken() {
            UpdateStamp();
            TimeTaken = UpdatedAt - CreatedAt;
        }

        public void Fail() {
            Action = RepoStatus.Failed;
            Failed = true;
            Color = "Red";
        }

        void UpdateData(int progress, int speed) {
            Progress = progress;
            Speed = speed;
            //StatusInfo = new StatusInfo(progress, speed);
        }

        public class StatusInfo
        {
            public StatusInfo(double progress, long speed) {
                Speed = speed;
                Progress = progress;
            }

            public double Progress { get; private set; }
            public long Speed { get; private set; }
        }

        //STR_NO = "No relevent local data found - I will be downloading the whole file"
        //STR_NO2 = "#{STR_NO}\n"
        //RX_NO = /^#{Regexp.escape(STR_NO)}/

        #endregion
    }
}