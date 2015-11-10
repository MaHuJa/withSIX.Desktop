// <copyright company="SIX Networks GmbH" file="QueueItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Input;
using ReactiveUI;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Queue
{
    public class QueueItemViewModel : ViewModel, IQueueItemViewModel
    {
        // TODO
        IReactiveCommand _abort;
        // TODO
        IReactiveCommand _pause;
        double _progress;
        double _speed;
        QueueItemState _state;

        public QueueItemViewModel(string name, double size) {
            Name = name;
            Size = size;
        }

        public string Name { get; }
        public double Size { get; }
        public double Speed
        {
            get { return _speed; }
            set { this.RaiseAndSetIfChanged(ref _speed, value); }
        }
        public ICommand Pause => _pause;
        public ICommand Abort => _abort;
        public QueueItemState State
        {
            get { return _state; }
            set { this.RaiseAndSetIfChanged(ref _state, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
    }

    public enum QueueItemState
    {
        Scheduled,
        Downloading,
        Completed
    }

    public interface IQueueItemViewModel
    {
        QueueItemState State { get; }
        double Progress { get; }
        string Name { get; }
        double Size { get; }
        double Speed { get; }
        ICommand Pause { get; }
        ICommand Abort { get; }
    }
}