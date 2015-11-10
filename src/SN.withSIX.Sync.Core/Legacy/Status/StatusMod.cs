// <copyright company="SIX Networks GmbH" file="StatusMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Legacy.Status
{
    [DoNotObfuscateType]
    public class StatusMod : ModelBase, IHaveTimestamps
    {
        ActionState _action;
        bool _completed;
        string _name = String.Empty;
        StatusRepo _repo;
        TimeSpan? _timeTaken;

        StatusMod() {
            CreatedAt = Tools.Generic.GetCurrentUtcDateTime;
            UpdatedAt = Tools.Generic.GetCurrentUtcDateTime;
        }

        public StatusMod(string mod) : this() {
            _name = mod;
        }

        #region StatusMod Members

        public TimeSpan? TimeTaken
        {
            get { return _timeTaken; }
            set { SetProperty(ref _timeTaken, value); }
        }

        public StatusRepo Repo
        {
            get { return _repo; }
            set { SetProperty(ref _repo, value); }
        }

        public bool Completed
        {
            get { return _completed; }
            set
            {
                UpdateTimeTaken();
                CalcFileSizes();
                var repo = Repo;
                if (repo != null)
                    repo.Finish();
                Action = ActionState.Ready;
                SetProperty(ref _completed, value);
            }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ActionState Action
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }

        public void UpdateStamp() {
            UpdatedAt = Tools.Generic.GetCurrentUtcDateTime;
        }

        public void UpdateTimeTaken() {
            UpdateStamp();
            TimeTaken = UpdatedAt - CreatedAt;
        }

        public void CalcFileSizes() {
            var repo = Repo;
            if (repo != null)
                repo.CalcFileSizes();
        }

        public void Abort() {
            var repo = Repo;
            if (repo != null)
                repo.Abort();
        }

        #endregion
    }

    public enum ActionState
    {
        Init,
        Ready
    }
}