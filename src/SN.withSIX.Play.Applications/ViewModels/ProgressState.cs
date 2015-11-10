// <copyright company="SIX Networks GmbH" file="ProgressState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public class ProgressState : PropertyChangedBase, IProgressState
    {
        bool _active;
        bool _isIndeterminate;
        double _maximum;
        double _progress;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { SetProperty(ref _isIndeterminate, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }
        public double Maximum
        {
            get { return _maximum; }
            set { SetProperty(ref _maximum, value); }
        }
    }
}